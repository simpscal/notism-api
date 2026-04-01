using MediatR;

namespace Notism.Application.Order.CreateOrder;

public class CreateOrderRequest : IRequest<CreateOrderResponse>
{
    public Guid UserId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public List<Guid> CartItemIds { get; set; } = new();
}