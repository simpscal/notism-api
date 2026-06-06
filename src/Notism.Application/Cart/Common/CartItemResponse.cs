using Notism.Domain.Cart;
using Notism.Shared.Extensions;

namespace Notism.Application.Cart.Common;

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

    public static CartItemResponse FromDomain(
        CartItem cartItem,
        Domain.Food.Food food,
        string imageUrl,
        List<CartItemCustomisationResponse> customisations)
    {
        return new CartItemResponse
        {
            Id = cartItem.Id,
            FoodId = cartItem.FoodId,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            DiscountPrice = food.DiscountPrice,
            ImageUrl = imageUrl,
            Category = food.Category?.Name ?? string.Empty,
            Quantity = cartItem.Quantity,
            StockQuantity = food.StockQuantity,
            QuantityUnit = food.QuantityUnit.GetStringValue(),
            Customisations = customisations,
            TotalSurcharge = cartItem.TotalSurcharge,
        };
    }
}