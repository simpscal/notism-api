using MediatR;

namespace Notism.Application.Order.CancelOrder;

public class CancelOrderRequest : IRequest
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}