using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.AdminUpdateFood;

public class AdminUpdateFoodHandler : IRequestHandler<AdminUpdateFoodRequest, AdminUpdateFoodResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<AdminUpdateFoodHandler> _logger;

    public AdminUpdateFoodHandler(
        IFoodRepository foodRepository,
        ICategoryRepository categoryRepository,
        IStorageService storageService,
        ILogger<AdminUpdateFoodHandler> logger)
    {
        _foodRepository = foodRepository;
        _categoryRepository = categoryRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<AdminUpdateFoodResponse> Handle(
        AdminUpdateFoodRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Notism.Domain.Food.Food>(f => f.Id == request.FoodId && !f.IsDeleted)
            .Include(f => f.Images)
            .Include("Category");
        var food = await _foodRepository.FindByExpressionAsync(specification);
        if (food == null)
        {
            throw new ResultFailureException("Food not found");
        }

        var name = request.Name ?? food.Name;
        var description = request.Description ?? food.Description;
        var price = request.Price ?? food.Price;
        Category? category = null;

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var categoryName = request.Category.Trim();
            var categorySpec = new FilterSpecification<Domain.Food.Category>(
                c => c.Name == categoryName && !c.IsDeleted);
            category = await _categoryRepository.FindByExpressionAsync(categorySpec)
                ?? throw new ResultFailureException("Category not found.");
        }

        var quantityUnit = !string.IsNullOrWhiteSpace(request.QuantityUnit)
            ? request.QuantityUnit.ToEnum<QuantityUnit>()
            : food.QuantityUnit;
        var stockQuantity = request.StockQuantity ?? food.StockQuantity;
        var discountPrice = request.DiscountPrice;

        food.Update(
            name,
            description,
            price,
            category?.Id,
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

        return new AdminUpdateFoodResponse
        {
            Id = food.Id,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrls = GetImageUrls(food.Images),
            Category = category?.Name ?? food.Category?.Name ?? string.Empty,
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
