using MediatR;

namespace Notism.Application.Cart.AddBulkCartItems;

public record AddBulkCartItemsRequest : IRequest<AddBulkCartItemsResponse>
{
    public Guid UserId { get; set; }
    public List<CartItemRequest> Items { get; set; } = new();
}

public record CartItemRequest
{
    public Guid FoodId { get; set; }
    public int Quantity { get; set; }
}