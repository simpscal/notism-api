namespace Notism.Application.Order.Models;

public class DeliveryStatusTimingResponse
{
    public DateTime? OrderPlacedCompletedAt { get; set; }
    public DateTime? PreparingCompletedAt { get; set; }
    public DateTime? OnTheWayCompletedAt { get; set; }
    public DateTime? DeliveredCompletedAt { get; set; }
}