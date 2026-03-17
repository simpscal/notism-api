using MediatR;

namespace Notism.Application.User.AdminDeleteUser;

public record AdminDeleteUserRequest : IRequest
{
    public Guid TargetUserId { get; init; }
    public Guid CallerUserId { get; init; }
}