using Notism.Application.Auth.Common;
using Notism.Application.Common.Interfaces;

namespace Notism.Application.Auth.Login;

public class LoginResponse
{
    public required AuthenticationUserInfoResponse User { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }

    public static LoginResponse FromDomain(Domain.User.User user, TokenResult token)
    {
        return new LoginResponse
        {
            User = AuthenticationUserInfoResponse.FromDomain(user),
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
        };
    }
}
