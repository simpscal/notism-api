using MediatR;

namespace Notism.Application.Cart.AddCartItem;

public record AddCartItemRequest : IRequest<AddCartItemResponse>
{
    public Guid UserId { get; set; }
    public Guid FoodId { get; set; }
    public int Quantity { get; set; }
    public List<CartItemCustomisationSelection>? Customisations { get; set; }
}

public record CartItemCustomisationSelection
{
    public Guid GroupId { get; set; }
    public Guid OptionId { get; set; }
}
