namespace Notism.Shared.Configuration;

public class SepaySettings
{
    public const string SectionName = "SepaySettings";
    public string WebhookSecret { get; set; } = string.Empty;
}
