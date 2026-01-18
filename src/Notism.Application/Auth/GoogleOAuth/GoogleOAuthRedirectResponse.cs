namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthRedirectResponse
{
    public required string RedirectUrl { get; set; }
    public required string State { get; set; }
}