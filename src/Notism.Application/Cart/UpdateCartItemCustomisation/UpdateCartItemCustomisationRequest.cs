using MediatR;

namespace Notism.Application.Cart.UpdateCartItemCustomisation;

public record UpdateCartItemCustomisationRequest : IRequest<UpdateCartItemCustomisationResponse>
{
    public Guid CartItemId { get; set; }
    public Guid UserId { get; set; }
    public Guid CustomisationOptionId { get; set; }
}
