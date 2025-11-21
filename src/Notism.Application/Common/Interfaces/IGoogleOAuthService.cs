using System.Text.Json.Serialization;

namespace Notism.Application.Common.Interfaces;

public interface IGoogleOAuthService
{
    GoogleOAuthRedirectUrl GetRedirectUrl();

    Task<GoogleToken> ExchangeCodeForTokenAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<GoogleUserInfo> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}

public class GoogleOAuthRedirectUrl
{
    public required string RedirectUrl { get; set; }
    public required string State { get; set; }
}

public class GoogleToken
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}

public class GoogleUserInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("verified_email")]
    public bool? VerifiedEmail { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("given_name")]
    public string? GivenName { get; set; }

    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    [JsonPropertyName("picture")]
    public string? Picture { get; set; }
}

