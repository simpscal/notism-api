using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.AddFood;

public class AddFoodHandler : IRequestHandler<AddFoodRequest, AddFoodResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<AddFoodHandler> _logger;

    public AddFoodHandler(
        IFoodRepository foodRepository,
        IStorageService storageService,
        ILogger<AddFoodHandler> logger)
    {
        _foodRepository = foodRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<AddFoodResponse> Handle(
        AddFoodRequest request,
        CancellationToken cancellationToken)
    {
        var category = request.Category.ToEnum<FoodCategory>();
        var quantityUnit = request.QuantityUnit.ToEnum<QuantityUnit>();

        if (request.DiscountPrice.HasValue && request.DiscountPrice.Value >= request.Price)
        {
            throw new ResultFailureException("Discount price must be less than the original price");
        }

        var food = Domain.Food.Food.Create(
            request.Name,
            request.Description,
            request.Price,
            category,
            quantityUnit,
            request.StockQuantity,
            request.DiscountPrice);

        if (!request.IsAvailable)
        {
            food.SetAvailability(false);
        }

        if (request.Images != null && request.Images.Count > 0)
        {
            foreach (var img in request.Images)
            {
                food.AddImage(img.FileKey, img.DisplayOrder, img.AltText);
            }
        }

        _foodRepository.Add(food);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Added food {FoodId} with name {Name}", food.Id, food.Name);

        return new AddFoodResponse
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrls = GetImageUrls(food.Images),
            Category = food.Category.GetStringValue(),
            IsAvailable = food.IsAvailable,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
            StockQuantity = food.StockQuantity,
            CreatedAt = food.CreatedAt,
            UpdatedAt = food.UpdatedAt,
        };
    }

    private List<string> GetImageUrls(IReadOnlyCollection<FoodImage> images)
    {
        return images
            .OrderBy(img => img.DisplayOrder)
            .Select(img => _storageService.GetPublicUrl(img.FileKey))
            .ToList();
    }
}
