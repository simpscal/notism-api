using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.AdminAddFood;

public class AdminAddFoodHandler : IRequestHandler<AdminAddFoodRequest, AdminAddFoodResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<AdminAddFoodHandler> _logger;

    public AdminAddFoodHandler(
        IFoodRepository foodRepository,
        ICategoryRepository categoryRepository,
        IStorageService storageService,
        ILogger<AdminAddFoodHandler> logger)
    {
        _foodRepository = foodRepository;
        _categoryRepository = categoryRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<AdminAddFoodResponse> Handle(
        AdminAddFoodRequest request,
        CancellationToken cancellationToken)
    {
        var categoryName = request.Category.Trim();
        var categorySpec = new FilterSpecification<Domain.Food.Category>(
            c => c.Name == categoryName && !c.IsDeleted);
        var category = await _categoryRepository.FindByExpressionAsync(categorySpec)
            ?? throw new ResultFailureException("Category not found.");

        var quantityUnit = request.QuantityUnit.ToEnum<QuantityUnit>();

        if (request.DiscountPrice.HasValue && request.DiscountPrice.Value >= request.Price)
        {
            throw new ResultFailureException("Discount price must be less than the original price");
        }

        var food = Domain.Food.Food.Create(
            request.Name,
            request.Description,
            request.Price,
            category.Id,
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

        return new AdminAddFoodResponse
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrls = GetImageUrls(food.Images),
            Category = category.Name,
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
