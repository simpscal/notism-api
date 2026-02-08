using MediatR;

namespace Notism.Application.Cart.GetCartItemCount;

public record GetCartItemCountRequest : IRequest<GetCartItemCountResponse>
{
    public Guid UserId { get; set; }
}

