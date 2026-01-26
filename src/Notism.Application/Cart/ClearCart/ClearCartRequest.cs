using MediatR;

namespace Notism.Application.Cart.ClearCart;

public record ClearCartRequest : IRequest
{
    public Guid UserId { get; set; }
}

