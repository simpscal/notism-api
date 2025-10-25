using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Auth.ResetPassword;

public class ResetPasswordRequest : IRequest<Result<ResetPasswordResponse>>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}