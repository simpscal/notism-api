namespace Notism.Application.Auth.Register;

public class RegisterResponse
{
    public required RegisterUserInfoResponse User { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}

public class RegisterUserInfoResponse
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}