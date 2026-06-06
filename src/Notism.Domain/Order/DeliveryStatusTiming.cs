namespace Notism.Domain.Order;

public class DeliveryStatusTiming
{
    public DateTime? OrderPlacedCompletedAt { get; set; }
    public DateTime? PreparingCompletedAt { get; set; }
    public DateTime? OnTheWayCompletedAt { get; set; }
    public DateTime? DeliveredCompletedAt { get; set; }
}