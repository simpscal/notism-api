using MediatR;

namespace Notism.Application.User.GetProfile;

public class GetUserProfileRequest : IRequest<GetUserProfileResponse>
{
    public Guid UserId { get; set; }
}