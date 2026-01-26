using MediatR;

namespace Notism.Application.Cart.RemoveCartItem;

public record RemoveCartItemRequest : IRequest
{
    public Guid UserId { get; set; }
    public Guid CartItemId { get; set; }
}

