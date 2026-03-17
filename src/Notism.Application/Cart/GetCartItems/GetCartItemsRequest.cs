using MediatR;

namespace Notism.Application.Cart.GetCartItems;

public record GetCartItemsRequest : IRequest<GetCartItemsResponse>
{
    public Guid UserId { get; set; }
}