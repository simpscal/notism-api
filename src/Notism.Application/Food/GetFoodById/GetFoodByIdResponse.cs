namespace Notism.Application.Food.GetFoodById;

public class GetFoodByIdResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required List<string> ImageUrls { get; set; }
    public required string Category { get; set; }
    public bool IsAvailable { get; set; }
    public required string QuantityUnit { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}