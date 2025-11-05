namespace Notism.Application.User.GetProfile;

public class GetUserProfileResponse
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
