using MediatR;

namespace Notism.Application.Cart.UpdateCartItemCustomisations;

public record UpdateCartItemCustomisationsRequest : IRequest<UpdateCartItemCustomisationsResponse>
{
    public Guid CartItemId { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemCustomisationSelection> Customisations { get; set; } = new();
}

public record CartItemCustomisationSelection
{
    public Guid GroupId { get; set; }
    public Guid OptionId { get; set; }
}
