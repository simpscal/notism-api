namespace Notism.Application.Auth.ResetPassword;

public sealed record ResetPasswordResponse
{
    public string Message { get; set; } = string.Empty;
}