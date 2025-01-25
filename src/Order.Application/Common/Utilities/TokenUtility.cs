using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Order.Application.Common.Models;
using Order.Domain.User;

namespace Order.Application.Common.Utilities;

public class TokenUtility(IConfiguration configuration)
{
    public JwtToken GenerateJwtToken(User user)
    {
        var accessTokenExpire = DateTime.UtcNow.AddMinutes(
            Convert.ToInt16(configuration["JwtSettings:TokenExpirationInMinutes"]));
        var refreshTokenExpire = DateTime.UtcNow.AddMinutes(
            Convert.ToInt16(configuration["JwtSettings:TokenExpirationInDays"]));

        return new JwtToken
        {
            AccessToken = GenerateToken(user, accessTokenExpire),
            RefreshToken = GenerateToken(user, refreshTokenExpire),
        };
    }

    private string GenerateToken(User user, DateTime expires)
    {
        var secretKey = configuration["JwtSettings:Secret"];

        if (secretKey == null)
        {
            throw new Exception("Empty JWTSettings Secret");
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject =
                new ClaimsIdentity([
                    new Claim("userId", user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                ]),
            Expires = expires,
            SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    SecurityAlgorithms.HmacSha256Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}