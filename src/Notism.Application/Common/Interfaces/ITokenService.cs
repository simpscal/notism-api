using Notism.Domain.User;

namespace Notism.Application.Common.Interfaces;

public interface ITokenService
{
    Task<TokenResult> GenerateTokenAsync(User user);
}

public class TokenResult
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}