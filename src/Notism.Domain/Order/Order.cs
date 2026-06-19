using Notism.Domain.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Events;
using Notism.Shared.Constants;
using Notism.Shared.Utilities;

namespace Notism.Domain.Order;

public class Order : AggregateRoot
{
    public static readonly TimeSpan RefundRequestWindow = TimeSpan.FromHours(24);

    public Guid UserId { get; private set; }
    public User.User? User { get; private set; }
    public string SlugId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DeliveryStatus DeliveryStatus { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? DeliveryNotes { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<DeliveryStatusHistory> _statusHistory = new();
    public IReadOnlyCollection<DeliveryStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    public Refund? Refund { get; private set; }

    private Order(
        Guid userId,
        PaymentMethod paymentMethod,
        List<Guid> cartItemIds,
        string? deliveryNotes = null)
    {
        UserId = userId;
        SlugId = SlugGenerator.Generate(Slugs.OrderPrefix);
        PaymentMethod = paymentMethod;
        DeliveryStatus = DeliveryStatus.OrderPlaced;
        PaymentStatus = PaymentStatus.Unpaid;
        DeliveryNotes = deliveryNotes;

        _statusHistory.Add(DeliveryStatusHistory.Create(Id, DeliveryStatus.OrderPlaced));

        AddDomainEvent(new OrderCreatedEvent(Id, UserId, TotalAmount, cartItemIds));
    }

    public static Order Create(Guid userId, PaymentMethod paymentMethod, List<Guid> cartItemIds, string? deliveryNotes = null)
    {
        return new Order(userId, paymentMethod, cartItemIds, deliveryNotes);
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
        AddDomainEvent(new OrderCancelledEvent(Id, UserId));
    }

    public Refund CreateRefund()
    {
        if (Refund != null)
        {
            throw new InvalidOperationException("Order already has a refund");
        }

        if (PaymentStatus != PaymentStatus.Paid)
        {
            throw new InvalidOperationException("Refund can only be created for a paid order");
        }

        if (PaymentMethod != PaymentMethod.Banking)
        {
            throw new InvalidOperationException("Refund is only supported for banking orders");
        }

        Refund = Refund.Create(Id, TotalAmount);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RefundCreatedEvent(Id, Refund.Id, UserId, Refund.Amount));

        return Refund;
    }

    public bool IsRefundRequestEligible(DateTime asOf)
    {
        if (Refund != null)
        {
            return false;
        }

        if (PaymentMethod is not (PaymentMethod.Banking or PaymentMethod.CashOnDelivery))
        {
            return false;
        }

        var deliveredAt = DeliveredAt();
        if (deliveredAt == null)
        {
            return false;
        }

        return asOf - deliveredAt.Value <= RefundRequestWindow;
    }

    public Refund RequestRefund()
    {
        if (Refund != null)
        {
            throw new InvalidOperationException("Order already has a refund");
        }

        Refund = Refund.Create(Id, TotalAmount);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RefundCreatedEvent(Id, Refund.Id, UserId, Refund.Amount));

        return Refund;
    }

    public void MarkRefundProcessing()
    {
        if (Refund == null)
        {
            throw new InvalidOperationException("Order has no refund to process");
        }

        Refund.MarkProcessing();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RefundProcessingEvent(Id, Refund.Id));
    }

    public void MarkRefundPaid(string transferReference)
    {
        if (Refund == null)
        {
            throw new InvalidOperationException("Order has no refund to mark as paid");
        }

        Refund.MarkPaid(transferReference);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RefundPaidEvent(Id, Refund.Id, UserId, Refund.PaidAt!.Value, transferReference));
    }

    public void MarkRefundFailed(string reason)
    {
        if (Refund == null)
        {
            throw new InvalidOperationException("Order has no refund to fail");
        }

        Refund.MarkFailed(reason);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new RefundFailedEvent(Id, Refund.Id, UserId, reason));
    }

    public void MarkAsPaid(DateTime paidAt)
    {
        PaymentStatus = PaymentStatus.Paid;
        PaidAt = paidAt;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderPaidEvent(Id, UserId, paidAt, SlugId));
    }

    public void MarkAsFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderPaymentFailedEvent(Id, UserId));
    }

    public void RevertToUnpaid()
    {
        PaymentStatus = PaymentStatus.Unpaid;
        PaidAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRefunded()
    {
        PaymentStatus = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePaymentStatus(PaymentStatus status)
    {
        if (PaymentStatus == status)
        {
            return;
        }

        switch (status)
        {
            case PaymentStatus.Paid:
                MarkAsPaid(DateTime.UtcNow);
                break;
            case PaymentStatus.Failed:
                MarkAsFailed();
                break;
            case PaymentStatus.Unpaid:
                RevertToUnpaid();
                break;
            case PaymentStatus.Refunded:
                MarkAsRefunded();
                break;
            default:
                throw new InvalidOperationException($"Unsupported payment status transition: {status}");
        }
    }

    public DeliveryStatusTiming GetDeliveryStatusTiming()
    {
        var history = _statusHistory
            .OrderBy(h => h.StatusChangedAt)
            .ToList();

        var timing = new DeliveryStatusTiming();

        var orderPlacedAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.OrderPlaced)?.StatusChangedAt;
        var preparingAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.Preparing)?.StatusChangedAt;
        var onTheWayAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.OnTheWay)?.StatusChangedAt;
        var deliveredAt = history.FirstOrDefault(h => h.Status == DeliveryStatus.Delivered)?.StatusChangedAt;

        if (orderPlacedAt.HasValue)
        {
            timing.OrderPlacedCompletedAt = orderPlacedAt.Value;
        }

        if (preparingAt.HasValue)
        {
            timing.OrderPlacedCompletedAt = preparingAt.Value;
        }

        if (onTheWayAt.HasValue)
        {
            timing.PreparingCompletedAt = onTheWayAt.Value;
        }

        if (deliveredAt.HasValue)
        {
            timing.OnTheWayCompletedAt = deliveredAt.Value;
        }

        if (deliveredAt.HasValue && DeliveryStatus == DeliveryStatus.Delivered)
        {
            timing.DeliveredCompletedAt = deliveredAt.Value;
        }

        return timing;
    }

    internal void RecordDeliveredAt(DateTime deliveredAt)
    {
        DeliveryStatus = DeliveryStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
        _statusHistory.Add(DeliveryStatusHistory.Create(Id, DeliveryStatus.Delivered, deliveredAt));
    }

    private DateTime? DeliveredAt()
        => _statusHistory
            .Where(h => h.Status == DeliveryStatus.Delivered)
            .OrderByDescending(h => h.StatusChangedAt)
            .Select(h => (DateTime?)h.StatusChangedAt)
            .FirstOrDefault();

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(item => item.TotalPrice);
    }

    private Order()
    {
    }
}