namespace Notism.Application.Cart.Models;

public record CartItemResponse
{
    public Guid Id { get; set; }
    public Guid FoodId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required string ImageUrl { get; set; }
    public required string Category { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public required string QuantityUnit { get; set; }
    public Guid? CustomisationGroupId { get; set; }
    public string? CustomisationGroupLabel { get; set; }
    public Guid? CustomisationOptionId { get; set; }
    public string? CustomisationLabel { get; set; }
    public decimal? Surcharge { get; set; }
    public List<CustomisationOptionResponse> CustomisationOptions { get; set; } = new();
}

public record CustomisationOptionResponse
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public decimal? Surcharge { get; set; }
}
