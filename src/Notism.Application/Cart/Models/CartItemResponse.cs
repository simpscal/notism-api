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
    public List<CartItemCustomisationResponse> Customisations { get; set; } = new();
    public decimal TotalSurcharge { get; set; }
}

public record CartItemCustomisationResponse
{
    public Guid? GroupId { get; set; }
    public required string GroupLabel { get; set; }
    public Guid? OptionId { get; set; }
    public required string OptionLabel { get; set; }
    public decimal? Surcharge { get; set; }
    public List<CartItemAvailableOptionResponse> AvailableOptions { get; set; } = new();
}

public record CartItemAvailableOptionResponse
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public decimal? Surcharge { get; set; }
}
