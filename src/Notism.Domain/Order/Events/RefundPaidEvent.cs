using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class RefundPaidEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid RefundId { get; }
    public Guid UserId { get; }
    public DateTime PaidAt { get; }
    public string TransferReference { get; }

    public RefundPaidEvent(Guid orderId, Guid refundId, Guid userId, DateTime paidAt, string transferReference)
    {
        OrderId = orderId;
        RefundId = refundId;
        UserId = userId;
        PaidAt = paidAt;
        TransferReference = transferReference;
    }
}