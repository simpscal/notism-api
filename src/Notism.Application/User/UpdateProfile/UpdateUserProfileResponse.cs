using Notism.Shared.Extensions;

namespace Notism.Application.User.UpdateProfile;

public sealed record UpdateUserProfileResponse
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Location { get; set; }
    public string Message { get; set; } = string.Empty;

    public static UpdateUserProfileResponse FromDomain(Domain.User.User user, string message)
    {
        return new UpdateUserProfileResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email?.Value ?? string.Empty,
            Role = user.Role.ToCamelCase(),
            AvatarUrl = user.AvatarUrl,
            Location = user.Location,
            Message = message,
        };
    }
}