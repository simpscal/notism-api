using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class RefundFailedEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid RefundId { get; }
    public Guid UserId { get; }
    public string Reason { get; }

    public RefundFailedEvent(Guid orderId, Guid refundId, Guid userId, string reason)
    {
        OrderId = orderId;
        RefundId = refundId;
        UserId = userId;
        Reason = reason;
    }
}