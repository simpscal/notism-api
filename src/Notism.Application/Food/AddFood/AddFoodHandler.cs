using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Food;
using Notism.Domain.Food.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.AddFood;

public class AddFoodHandler : IRequestHandler<AddFoodRequest, AddFoodResponse>
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<AddFoodHandler> _logger;

    public AddFoodHandler(
        IFoodRepository foodRepository,
        ILogger<AddFoodHandler> logger)
    {
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task<AddFoodResponse> Handle(
        AddFoodRequest request,
        CancellationToken cancellationToken)
    {
        var category = request.Category.ToEnum<FoodCategory>();
        var quantityUnit = request.QuantityUnit.ToEnum<QuantityUnit>();

        var food = Domain.Food.Food.Create(
            request.Name,
            request.Description,
            request.Price,
            category,
            quantityUnit,
            request.StockQuantity,
            request.DiscountPrice);

        // Add images to the food
        foreach (var images in request.Images)
        {
            food.AddImage(images.FileKey, images.DisplayOrder, images.AltText);
        }

        _foodRepository.Add(food);
        await _foodRepository.SaveChangesAsync();

        _logger.LogInformation("Created food {FoodId} with name {Name} and {ImageCount} images", food.Id, food.Name, request.Images.Count);

        return new AddFoodResponse
        {
            Id = food.Id,
        };
    }
}

