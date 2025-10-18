namespace Notism.Application.Auth.Register;

public class RegisterResponse
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}