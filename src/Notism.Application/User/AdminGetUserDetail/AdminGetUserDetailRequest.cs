using MediatR;

namespace Notism.Application.User.AdminGetUserDetail;

public record AdminGetUserDetailRequest : IRequest<AdminUserDetailResponse>
{
    public Guid UserId { get; set; }
}
