namespace Notism.Api.Endpoints;

public record CreateOrderPayload
{
    public string PaymentMethod { get; set; } = string.Empty;
    public List<Guid> CartItemIds { get; set; } = new();
    public string? DeliveryNotes { get; set; }
}