using MediatR;

namespace Notism.Application.Food.AddFood;

public record AddFoodRequest : IRequest<AddFoodResponse>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public required string Category { get; set; }
    public required string QuantityUnit { get; set; }
    public int StockQuantity { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required List<FoodImageRequest> Images { get; set; }
}

public record FoodImageRequest
{
    public required string FileKey { get; set; }
    public int DisplayOrder { get; set; }
    public string? AltText { get; set; }
}