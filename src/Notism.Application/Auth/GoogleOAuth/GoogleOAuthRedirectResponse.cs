using Notism.Application.Common.Services;

namespace Notism.Application.Auth.GoogleOAuth;

public sealed record GoogleOAuthRedirectResponse
{
    public required string RedirectUrl { get; set; }
    public required string State { get; set; }

    public static GoogleOAuthRedirectResponse FromDomain(GoogleOAuthRedirectUrl redirectUrl)
    {
        return new GoogleOAuthRedirectResponse
        {
            RedirectUrl = redirectUrl.RedirectUrl,
            State = redirectUrl.State,
        };
    }
}
