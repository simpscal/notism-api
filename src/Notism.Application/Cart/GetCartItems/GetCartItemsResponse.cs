using Notism.Application.Cart.Common;

namespace Notism.Application.Cart.GetCartItems;

public record GetCartItemsResponse
{
    public required List<CartItemResponse> Items { get; set; }
}