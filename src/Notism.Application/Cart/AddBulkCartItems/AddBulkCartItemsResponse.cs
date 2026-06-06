using Notism.Application.Cart.Common;

namespace Notism.Application.Cart.AddBulkCartItems;

public record AddBulkCartItemsResponse
{
    public required List<CartItemResponse> Items { get; set; }
}