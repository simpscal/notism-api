using Notism.Shared.Models;

namespace Notism.Application.Food.GetFoods;

public record GetFoodsResponse : PagedResult<FoodItemResponse>;

public record FoodItemResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required string ImageUrl { get; set; }
    public required string Category { get; set; }
    public bool IsAvailable { get; set; }
    public required string QuantityUnit { get; set; }
    public int StockQuantity { get; set; }
}