using Notism.Domain.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Events;

namespace Notism.Domain.Order;

public class Order : AggregateRoot
{
    public Guid UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DeliveryStatus DeliveryStatus { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<DeliveryStatusHistory> _statusHistory = new();
    public IReadOnlyCollection<DeliveryStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Order(
        Guid userId,
        PaymentMethod paymentMethod,
        List<Guid> cartItemIds)
    {
        UserId = userId;
        PaymentMethod = paymentMethod;
        DeliveryStatus = DeliveryStatus.OrderPlaced;

        _statusHistory.Add(DeliveryStatusHistory.Create(Id, DeliveryStatus.OrderPlaced));

        AddDomainEvent(new OrderCreatedEvent(Id, UserId, TotalAmount, cartItemIds));
    }

    public static Order Create(Guid userId, PaymentMethod paymentMethod, List<Guid> cartItemIds)
    {
        return new Order(userId, paymentMethod, cartItemIds);
    }

    public void AddItem(OrderItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        _items.Add(item);
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDeliveryStatus(DeliveryStatus status)
    {
        if (DeliveryStatus == status)
        {
            return;
        }

        DeliveryStatus = status;
        UpdatedAt = DateTime.UtcNow;

        _statusHistory.Add(DeliveryStatusHistory.Create(Id, status));

        ClearDomainEvents();
        AddDomainEvent(new DeliveryStatusUpdatedEvent(Id, UserId, status));
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(item => item.TotalPrice);
    }

    private Order()
    {
    }
}