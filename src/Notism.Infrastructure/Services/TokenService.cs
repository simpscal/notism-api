using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Notism.Application.Common.Interfaces;
using Notism.Domain.RefreshToken;
using Notism.Domain.RefreshToken.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Configuration;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
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
        var refreshTokenSpec = new RefreshTokenByTokenSpecification(refreshToken);
        var refreshTokenEntity = await _refreshTokenRepository.FindByExpressionAsync(refreshTokenSpec);

        if (refreshTokenEntity == null || !refreshTokenEntity.IsValid())
        {
            throw new InvalidRefreshTokenException("Invalid refresh token");
        }

        var user = await _userRepository.FindByExpressionAsync(new UserByIdSpecification(refreshTokenEntity.UserId)) ?? throw new InvalidRefreshTokenException("User not found");

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