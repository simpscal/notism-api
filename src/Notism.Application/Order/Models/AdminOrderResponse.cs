namespace Notism.Application.Order.Models;

public class AdminOrderResponse
{
    public Guid Id { get; set; }
    public string SlugId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalItems { get; set; }
}