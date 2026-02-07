namespace Notism.Application.Order.Models;

public class OrderResponse
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public DeliveryStatusTimingResponse DeliveryStatusTiming { get; set; } = new();
}