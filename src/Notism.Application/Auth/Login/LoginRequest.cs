using System.ComponentModel.DataAnnotations;
using MediatR;
using Notism.Shared.Models;

namespace Notism.Application.Auth.Login;

public class LoginRequest : IRequest<Result<LoginResponse>>
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
    public required string Password { get; set; }
}