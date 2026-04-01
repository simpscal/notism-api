using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class OrderCreatedEvent : DomainEvent, INotification
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public decimal TotalAmount { get; }
    public List<Guid> CartItemIds { get; }

    public OrderCreatedEvent(Guid orderId, Guid userId, decimal totalAmount, List<Guid> cartItemIds)
    {
        OrderId = orderId;
        UserId = userId;
        TotalAmount = totalAmount;
        CartItemIds = cartItemIds;
    }
}