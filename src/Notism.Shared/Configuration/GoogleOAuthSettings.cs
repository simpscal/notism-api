namespace Notism.Shared.Configuration;

public class GoogleOAuthSettings
{
    public const string SectionName = "GoogleOAuth";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectPath { get; set; } = "/auth/oauth/google/callback";
}