using MediatR;

namespace Notism.Application.Auth.Login;

public class LoginRequest : IRequest<(LoginResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}