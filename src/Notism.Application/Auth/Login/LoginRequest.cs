using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Auth.Login;

public class LoginRequest : IRequest<Result<(LoginResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>>
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}