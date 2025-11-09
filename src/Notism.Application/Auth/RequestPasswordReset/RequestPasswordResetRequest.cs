using MediatR;

namespace Notism.Application.Auth.RequestPasswordReset;

public class RequestPasswordResetRequest : IRequest<RequestPasswordResetResponse>
{
    public string Email { get; set; } = string.Empty;
}