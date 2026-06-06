using Notism.Shared.Extensions;

namespace Notism.Application.Auth.Common;

public class AuthenticationUserInfoResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;

    public static AuthenticationUserInfoResponse FromDomain(Domain.User.User user)
    {
        return new AuthenticationUserInfoResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToCamelCase(),
        };
    }
}