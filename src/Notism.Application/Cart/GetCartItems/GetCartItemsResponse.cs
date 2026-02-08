using Notism.Application.Cart.Models;

namespace Notism.Application.Cart.GetCartItems;

public record GetCartItemsResponse
{
    public required List<CartItemResponse> Items { get; set; }
}