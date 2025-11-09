using MediatR;

namespace Notism.Application.Auth.ResetPassword;

public class ResetPasswordRequest : IRequest<ResetPasswordResponse>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}