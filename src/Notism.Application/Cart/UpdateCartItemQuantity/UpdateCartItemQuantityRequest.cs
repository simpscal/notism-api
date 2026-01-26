using MediatR;

namespace Notism.Application.Cart.UpdateCartItemQuantity;

public record UpdateCartItemQuantityRequest : IRequest<UpdateCartItemQuantityResponse>
{
    public Guid UserId { get; set; }
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}

