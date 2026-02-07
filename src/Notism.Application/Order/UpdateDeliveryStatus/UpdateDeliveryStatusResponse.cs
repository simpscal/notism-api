namespace Notism.Application.Order.UpdateDeliveryStatus;

public class UpdateDeliveryStatusResponse
{
    public Guid OrderId { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}