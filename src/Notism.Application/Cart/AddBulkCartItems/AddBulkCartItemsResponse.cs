using Notism.Application.Cart.Models;

namespace Notism.Application.Cart.AddBulkCartItems;

public record AddBulkCartItemsResponse
{
    public required List<CartItemResponse> Items { get; set; }
}

