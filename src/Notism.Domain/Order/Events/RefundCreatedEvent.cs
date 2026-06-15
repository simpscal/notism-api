using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class RefundCreatedEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid RefundId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }

    public RefundCreatedEvent(Guid orderId, Guid refundId, Guid userId, decimal amount)
    {
        OrderId = orderId;
        RefundId = refundId;
        UserId = userId;
        Amount = amount;
    }
}