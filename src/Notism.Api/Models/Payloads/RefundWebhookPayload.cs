using System.Text.Json.Serialization;

namespace Notism.Api.Endpoints;

public record RefundWebhookPayload
{
    [JsonPropertyName("transfer_reference")]
    public string TransferReference { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("failure_reason")]
    public string? FailureReason { get; init; }
}