using MediatR;

using Notism.Application.Auth.Models;

namespace Notism.Application.Auth.Register;

public class RegisterRequest : IRequest<(AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    public required string Email { get; set; }

    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}