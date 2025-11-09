using MediatR;

namespace Notism.Application.User.UpdateProfile;

public class UpdateUserProfileRequest : IRequest<UpdateUserProfileResponse>
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}