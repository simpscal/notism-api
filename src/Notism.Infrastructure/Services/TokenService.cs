using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Notism.Application.Common.Interfaces;
using Notism.Domain.User;

namespace Notism.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<TokenResult> GenerateTokenAsync(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "Notism.Api";
        var audience = _configuration["JwtSettings:Audience"] ?? "Notism.Client";
        var expirationMinutes = int.Parse(_configuration["JwtSettings:TokenExpirationInMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        var result = new TokenResult
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
        };

        return Task.FromResult(result);
    }
}