using Notism.Domain.Common;
using Notism.Domain.Order.Enums;

namespace Notism.Domain.Order.Events;

public class DeliveryStatusUpdatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public DeliveryStatus Status { get; }

    public DeliveryStatusUpdatedEvent(Guid orderId, Guid userId, DeliveryStatus status)
    {
        OrderId = orderId;
        UserId = userId;
        Status = status;
    }
}