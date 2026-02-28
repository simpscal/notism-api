using MediatR;

namespace Notism.Application.Food.AdminUpdateFood;

public record AdminUpdateFoodRequest : IRequest<AdminUpdateFoodResponse>
{
    public Guid FoodId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? Category { get; set; }
    public bool? IsAvailable { get; set; }
    public int? StockQuantity { get; set; }
    public string? QuantityUnit { get; set; }
    public List<FoodImageRequest>? Images { get; set; }
}

public record FoodImageRequest
{
    public required string FileKey { get; set; }
    public int DisplayOrder { get; set; }
    public string? AltText { get; set; }
}
