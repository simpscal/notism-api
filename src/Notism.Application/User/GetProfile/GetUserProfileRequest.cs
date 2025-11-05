using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.User.GetProfile;

public class GetUserProfileRequest : IRequest<Result<GetUserProfileResponse>>
{
    public Guid UserId { get; set; }
}
