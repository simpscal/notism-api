using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class OrderCancelledEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid UserId { get; }

    public OrderCancelledEvent(Guid orderId, Guid userId)
    {
        OrderId = orderId;
        UserId = userId;
    }
}