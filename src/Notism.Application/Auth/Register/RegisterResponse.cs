using Notism.Application.Auth.Common;
using Notism.Application.Common.Interfaces;

namespace Notism.Application.Auth.Register;

public class RegisterResponse
{
    public required AuthenticationUserInfoResponse User { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }

    public static RegisterResponse FromDomain(Domain.User.User user, TokenResult token)
    {
        return new RegisterResponse
        {
            User = AuthenticationUserInfoResponse.FromDomain(user),
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
        };
    }
}
