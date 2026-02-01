using MediatR;

namespace Notism.Application.Food.UpdateFood;

public record UpdateFoodRequest : IRequest<UpdateFoodResponse>
{
    public Guid FoodId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public string? QuantityUnit { get; set; }
    public int? StockQuantity { get; set; }
    public decimal? DiscountPrice { get; set; }
    public List<FoodImageRequest>? Images { get; set; }
}

public record FoodImageRequest
{
    public required string FileKey { get; set; }
    public int DisplayOrder { get; set; }
    public string? AltText { get; set; }
}

