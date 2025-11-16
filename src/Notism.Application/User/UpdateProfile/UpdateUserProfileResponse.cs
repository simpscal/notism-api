namespace Notism.Application.User.UpdateProfile;

public class UpdateUserProfileResponse
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Message { get; set; } = string.Empty;
}