namespace Notism.Application.Auth.Login;

public class LoginResponse
{
    public required LoginUserInfoResponse User { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class LoginUserInfoResponse
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}