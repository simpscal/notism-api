using Notism.Application.Auth.Common;
using Notism.Application.Common.Interfaces;

namespace Notism.Application.Auth.GoogleOAuth;

public sealed record GoogleOAuthCallbackResponse
{
    public required AuthenticationUserInfoResponse User { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }

    public static GoogleOAuthCallbackResponse FromDomain(Domain.User.User user, TokenResult token)
    {
        return new GoogleOAuthCallbackResponse
        {
            User = AuthenticationUserInfoResponse.FromDomain(user),
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
        };
    }
}