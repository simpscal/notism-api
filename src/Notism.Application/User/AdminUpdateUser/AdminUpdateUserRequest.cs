using MediatR;

using Notism.Application.User.AdminGetUserDetail;

namespace Notism.Application.User.AdminUpdateUser;

public record AdminUpdateUserRequest : IRequest<AdminUserDetailResponse>
{
    public Guid TargetUserId { get; set; }
    public Guid CallerUserId { get; set; }
    public string Role { get; set; } = string.Empty;
}