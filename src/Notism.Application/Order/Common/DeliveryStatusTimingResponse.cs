using Notism.Domain.Order;

namespace Notism.Application.Order.Common;

public class DeliveryStatusTimingResponse
{
    public DateTime? OrderPlacedCompletedAt { get; set; }
    public DateTime? PreparingCompletedAt { get; set; }
    public DateTime? OnTheWayCompletedAt { get; set; }
    public DateTime? DeliveredCompletedAt { get; set; }

    public static DeliveryStatusTimingResponse FromDomain(DeliveryStatusTiming timing)
    {
        return new DeliveryStatusTimingResponse
        {
            OrderPlacedCompletedAt = timing.OrderPlacedCompletedAt,
            PreparingCompletedAt = timing.PreparingCompletedAt,
            OnTheWayCompletedAt = timing.OnTheWayCompletedAt,
            DeliveredCompletedAt = timing.DeliveredCompletedAt,
        };
    }
}