namespace Notism.Api.Endpoints;

public record AdminUpdateOrderPaymentStatusPayload
{
    public string PaymentStatus { get; set; } = string.Empty;
}