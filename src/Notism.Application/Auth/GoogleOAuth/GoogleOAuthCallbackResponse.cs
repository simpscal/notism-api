using Notism.Application.Auth.Common;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthCallbackResponse
{
    public required AuthenticationUserInfoResponse User { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}