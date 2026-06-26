using MediatR;

using Notism.Domain.Common;

namespace Notism.Domain.Order.Events;

public class OrderCreatedEvent : DomainEvent, INotification
{
    public string SlugId { get; }
    public Guid UserId { get; }
    public decimal TotalAmount { get; }
    public List<Guid> CartItemIds { get; }

    public OrderCreatedEvent(string slugId, Guid userId, decimal totalAmount, List<Guid> cartItemIds)
    {
        SlugId = slugId;
        UserId = userId;
        TotalAmount = totalAmount;
        CartItemIds = cartItemIds;
    }
}