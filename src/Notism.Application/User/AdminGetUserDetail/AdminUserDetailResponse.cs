using Notism.Shared.Extensions;

namespace Notism.Application.User.AdminGetUserDetail;

public class AdminUserDetailResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }

    public static AdminUserDetailResponse FromDomain(Domain.User.User user)
    {
        return new AdminUserDetailResponse
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email.Value,
            Role = user.Role.ToCamelCase(),
            PhoneNumber = null,
            Location = null,
            CreatedAt = user.CreatedAt,
        };
    }
}
