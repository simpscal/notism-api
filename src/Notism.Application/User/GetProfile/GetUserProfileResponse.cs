using Notism.Shared.Extensions;

namespace Notism.Application.User.GetProfile;

public class GetUserProfileResponse
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Location { get; set; }

    public static GetUserProfileResponse FromDomain(Domain.User.User user, string avatarUrl)
    {
        return new GetUserProfileResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToCamelCase(),
            AvatarUrl = avatarUrl,
            Location = user.Location,
        };
    }
}
