using Notism.Shared.Extensions;
using Notism.Shared.Models;

namespace Notism.Application.User.AdminGetUsers;

public record AdminGetUsersResponse : PagedResult<AdminGetUsersItemResponse>;

public record AdminGetUsersItemResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Location { get; set; }
    public string AuthType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static AdminGetUsersItemResponse FromDomain(Domain.User.User user)
    {
        return new AdminGetUsersItemResponse
        {
            Id = user.Id,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email.Value,
            Role = user.Role.ToCamelCase(),
            PhoneNumber = null,
            Location = null,
            AuthType = "email",
            CreatedAt = user.CreatedAt,
        };
    }
}
