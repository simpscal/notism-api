using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class OrderPaidEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public DateTime PaidAt { get; }
    public string SlugId { get; }

    public OrderPaidEvent(Guid orderId, Guid userId, DateTime paidAt, string slugId)
    {
        OrderId = orderId;
        UserId = userId;
        PaidAt = paidAt;
        SlugId = slugId;
    }
}
