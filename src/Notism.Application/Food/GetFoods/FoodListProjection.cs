using Notism.Domain.Food.Enums;

namespace Notism.Application.Food.GetFoods;

public record FoodListProjection
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public decimal Price { get; init; }
    public decimal? DiscountPrice { get; init; }
    public required string CategoryName { get; init; }
    public bool IsAvailable { get; init; }
    public QuantityUnit QuantityUnit { get; init; }
    public int StockQuantity { get; init; }
    public required List<ImageKeyOrder> ImageKeys { get; init; }
}

public record ImageKeyOrder(string FileKey, int DisplayOrder);
