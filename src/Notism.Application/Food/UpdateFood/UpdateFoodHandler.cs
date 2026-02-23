using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.UpdateFood;

public class UpdateFoodHandler : IRequestHandler<UpdateFoodRequest, UpdateFoodResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<UpdateFoodHandler> _logger;

    public UpdateFoodHandler(
        IFoodRepository foodRepository,
        IStorageService storageService,
        ILogger<UpdateFoodHandler> logger)
    {
        _foodRepository = foodRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<UpdateFoodResponse> Handle(
        UpdateFoodRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include(f => f.Images);
        var food = await _foodRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException("Food not found");

        var name = request.Name ?? food.Name;
        var description = request.Description ?? food.Description;
        var price = request.Price ?? food.Price;
        var category = !string.IsNullOrWhiteSpace(request.Category)
            ? request.Category.ToEnum<FoodCategory>()
            : food.Category;
        var quantityUnit = !string.IsNullOrWhiteSpace(request.QuantityUnit)
            ? request.QuantityUnit.ToEnum<QuantityUnit>()
            : food.QuantityUnit;
        var stockQuantity = request.StockQuantity ?? food.StockQuantity;
        var discountPrice = request.DiscountPrice;

        if (discountPrice.HasValue && discountPrice.Value >= price)
        {
            throw new ResultFailureException("Discount price must be less than the original price");
        }

        food.Update(
            name,
            description,
            price,
            category,
            quantityUnit,
            stockQuantity,
            discountPrice);

        if (request.IsAvailable.HasValue)
        {
            food.SetAvailability(request.IsAvailable.Value);
        }

        if (request.Images != null && request.Images.Count > 0)
        {
            var images = request.Images.Select(img => (img.FileKey, img.DisplayOrder, img.AltText));
            food.UpdateImages(images);
        }
        else
        {
            food.ClearImages();
        }

        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Updated food {FoodId} with name {Name}", food.Id, food.Name);

        return new UpdateFoodResponse
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
