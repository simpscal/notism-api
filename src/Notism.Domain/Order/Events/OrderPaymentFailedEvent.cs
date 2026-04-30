using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class OrderPaymentFailedEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid UserId { get; }

    public OrderPaymentFailedEvent(Guid orderId, Guid userId)
    {
        OrderId = orderId;
        UserId = userId;
    }
}
