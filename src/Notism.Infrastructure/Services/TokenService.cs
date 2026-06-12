using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.RefreshToken;
using Notism.Domain.RefreshToken.Repositories;
using Notism.Domain.User;
using Notism.Shared.Configuration;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IReadDbContext _readDbContext;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IReadDbContext readDbContext)
    {
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
        _readDbContext = readDbContext;
    }

    public async Task<TokenResult> GenerateTokenAsync(Domain.User.User user)
    {
        var secret = _jwtSettings.Secret;
        var issuer = _jwtSettings.Issuer;
        var audience = _jwtSettings.Audience;
        var expirationMinutes = _jwtSettings.TokenExpirationInMinutes;
        var refreshExpirationDays = _jwtSettings.RefreshTokenExpirationInDays;

        var key = Encoding.ASCII.GetBytes(secret);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToCamelCase()),
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

        // Generate refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshExpirationDays);
        var refreshTokenEntity = RefreshToken.Create(refreshToken, user.Id, refreshTokenExpiresAt);

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        var result = new TokenResult
        {
            Token = tokenString,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
        };

        return result;
    }

    public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
    {
        var refreshTokenEntity = await _readDbContext.Set<RefreshToken>()
            .Where(rt => rt.Token == refreshToken)
            .FirstOrDefaultAsync();

        if (refreshTokenEntity == null || !refreshTokenEntity.IsValid())
        {
            throw new InvalidRefreshTokenException("Invalid refresh token");
        }

        var user = await _readDbContext.Set<User>()
                .Where(u => u.Id == refreshTokenEntity.UserId)
                .FirstOrDefaultAsync()
            ?? throw new InvalidRefreshTokenException("User not found");

        return await GenerateTokenAsync(user);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }
}