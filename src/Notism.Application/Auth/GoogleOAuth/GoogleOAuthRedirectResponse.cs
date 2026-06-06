using Notism.Application.Common.Interfaces;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthRedirectResponse
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
