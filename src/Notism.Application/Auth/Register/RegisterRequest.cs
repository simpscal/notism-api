using MediatR;
using Notism.Shared.Models;

namespace Notism.Application.Auth.Register;

public class RegisterRequest : IRequest<Result<RegisterResponse>>
{
    public required string Email { get; set; }

    public required string Password { get; set; }
}