namespace Notism.Application.Auth.RequestPasswordReset;

public sealed record RequestPasswordResetResponse
{
    public string Message { get; set; } = string.Empty;
}