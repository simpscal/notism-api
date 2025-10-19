using MediatR;
using Notism.Shared.Models;

namespace Notism.Application.Auth.Login;

public class LoginRequest : IRequest<Result<LoginResponse>>
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}