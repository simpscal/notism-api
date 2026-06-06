namespace Notism.Api.Endpoints;

public record AdminUpdateOrderDeliveryStatusPayload
{
    public string DeliveryStatus { get; set; } = string.Empty;
}