namespace Notism.Shared.Configuration;

public class BankingRefundSettings
{
    public const string SectionName = "BankingRefundSettings";

    public string WebhookSecret { get; set; } = string.Empty;
    public string ProviderApiKey { get; set; } = string.Empty;
    public string ProviderBaseUrl { get; set; } = string.Empty;
    public bool AlwaysReject { get; set; }
    public string RejectReason { get; set; } = "Transfer rejected by provider";
}