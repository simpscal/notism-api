using Notism.Domain.User;

namespace Notism.Application.Common.Interfaces;

public interface ITokenService
{
    Task<TokenResult> GenerateTokenAsync(User user);
    Task<TokenResult> RefreshTokenAsync(string refreshToken);
}

public class TokenResult
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
}