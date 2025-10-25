using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Auth.RequestPasswordReset;

public class RequestPasswordResetRequest : IRequest<Result<RequestPasswordResetResponse>>
{
    public string Email { get; set; } = string.Empty;
}