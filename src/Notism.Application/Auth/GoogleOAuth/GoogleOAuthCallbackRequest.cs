using MediatR;

using Notism.Application.Common.Models;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthCallbackRequest : IRequest<(AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    public required string Code { get; set; }
    public required string State { get; set; }
}