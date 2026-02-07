using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.UpdateFood;

public class UpdateFoodHandler : IRequestHandler<UpdateFoodRequest, UpdateFoodResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<UpdateFoodHandler> _logger;

    public UpdateFoodHandler(
        IFoodRepository foodRepository,
        ILogger<UpdateFoodHandler> logger)
    {
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task<UpdateFoodResponse> Handle(
        UpdateFoodRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Food.Food>(f => f.Id == request.FoodId);
        var food = await _foodRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException("Food not found");

        // Use provided values or keep existing values
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

        // Validate discount price against the price being used (provided or existing)
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

        // Update images only if provided
        if (request.Images != null && request.Images.Any())
        {
            var images = request.Images.Select(img => (img.FileKey, img.DisplayOrder, img.AltText));
            food.UpdateImages(images);
        }

        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Updated food {FoodId} with name {Name}", food.Id, food.Name);

        return new UpdateFoodResponse
        {
            Id = food.Id,
        };
    }
}