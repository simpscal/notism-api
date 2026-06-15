using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class RefundProcessingEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid RefundId { get; }

    public RefundProcessingEvent(Guid orderId, Guid refundId)
    {
        OrderId = orderId;
        RefundId = refundId;
    }
}