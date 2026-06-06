using MediatR;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthCallbackRequest : IRequest<(GoogleOAuthCallbackResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    public required string Code { get; set; }
    public required string State { get; set; }
}