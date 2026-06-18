namespace Notism.Api.Endpoints;

public record MarkRefundFailedPayload
{
    public string Reason { get; set; } = string.Empty;
}