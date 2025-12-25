namespace Notism.Application.Common.Models;

public class AuthenticationResponse
{
    public required AuthenticationUserInfoResponse User { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class AuthenticationUserInfoResponse
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
}