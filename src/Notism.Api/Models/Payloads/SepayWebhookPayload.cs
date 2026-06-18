using System.Text.Json.Serialization;

namespace Notism.Api.Endpoints;

public record SepayWebhookPayload
{
    [JsonPropertyName("id")]
    public long TransactionId { get; init; }

    [JsonPropertyName("transferAmount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("transactionDate")]
    public string TransactionDate { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    [JsonPropertyName("transferType")]
    public string TransferType { get; init; } = string.Empty;
}