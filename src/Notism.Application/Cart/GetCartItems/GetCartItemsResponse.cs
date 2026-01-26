namespace Notism.Application.Cart.GetCartItems;

public record GetCartItemsResponse
{
    public required List<CartItemResponse> Items { get; set; }
}

public record CartItemResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public required string ImageUrl { get; set; }
    public required string Category { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public required string QuantityUnit { get; set; }
}

