using Notism.Domain.Common;
using Notism.Domain.Order.Enums;

namespace Notism.Domain.Order;

public class DeliveryStatusHistory : Entity
{
    public Guid OrderId { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DateTime StatusChangedAt { get; private set; }
    public Order Order { get; private set; } = null!;

    private DeliveryStatusHistory(
        Guid orderId,
        DeliveryStatus status,
        DateTime statusChangedAt)
    {
        OrderId = orderId;
        Status = status;
        StatusChangedAt = statusChangedAt;
    }

    public static DeliveryStatusHistory Create(Guid orderId, DeliveryStatus status)
    {
        return new DeliveryStatusHistory(orderId, status, DateTime.UtcNow);
    }

    private DeliveryStatusHistory()
    {
    }
}