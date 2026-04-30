using Notism.Domain.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Events;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Constants;
using Notism.Shared.Utilities;

namespace Notism.Domain.Order;

public class Order : AggregateRoot
{
    public Guid UserId { get; private set; }
    public User.User? User { get; private set; }
    public string SlugId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DeliveryStatus DeliveryStatus { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTime? PaidAt { get; private set; }

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
        SlugId = SlugGenerator.Generate(Slugs.OrderPrefix);
        PaymentMethod = paymentMethod;
        DeliveryStatus = DeliveryStatus.OrderPlaced;
        PaymentStatus = PaymentStatus.Unpaid;

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

    public void Cancel()
    {
        if (DeliveryStatus != DeliveryStatus.OrderPlaced && DeliveryStatus != DeliveryStatus.Preparing)
        {
            throw new InvalidOperationException("Order can only be cancelled when delivery status is OrderPlaced or Preparing");
        }

        if (DeliveryStatus == DeliveryStatus.Cancelled)
        {
            return;
        }

        DeliveryStatus = DeliveryStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        _statusHistory.Add(DeliveryStatusHistory.Create(Id, DeliveryStatus.Cancelled));

        ClearDomainEvents();
        AddDomainEvent(new DeliveryStatusUpdatedEvent(Id, UserId, DeliveryStatus.Cancelled));
    }

    public void MarkAsPaid(DateTime paidAt)
    {
        PaymentStatus = PaymentStatus.Paid;
        PaidAt = paidAt;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderPaidEvent(Id, UserId, paidAt));
    }

    public void MarkAsFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderPaymentFailedEvent(Id, UserId));
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(item => item.TotalPrice);
    }

    private Order()
    {
    }
}