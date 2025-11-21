using MediatR;

using Notism.Application.Common.Models;

namespace Notism.Application.Auth.Login;

public class LoginRequest : IRequest<(AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}