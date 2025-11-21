using System.Security.Cryptography;

using Microsoft.Extensions.Configuration;

using Notism.Application.Common.Interfaces;
using Notism.Shared.Exceptions;

namespace Notism.Infrastructure.Services;

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly IHttpService _httpService;
    private readonly IConfiguration _configuration;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _appDomain;
    private readonly string _redirectPath;

    public GoogleOAuthService(IHttpService httpService, IConfiguration configuration)
    {
        _httpService = httpService;
        _configuration = configuration;
        _clientId = configuration["GoogleOAuth:ClientId"]!;
        _clientSecret = configuration["GoogleOAuth:ClientSecret"]!;
        _appDomain = configuration["ClientApp:Url"]!;
        _redirectPath = configuration["GoogleOAuth:RedirectPath"]!;
    }

    public GoogleOAuthRedirectUrl GetRedirectUrl()
    {
        var state = GenerateState();
        var redirectUri = $"{_appDomain}{_redirectPath}";
        var scopes = "openid email profile";
        var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(_clientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(scopes)}" +
            $"&state={Uri.EscapeDataString(state)}" +
            $"&access_type=offline" +
            $"&prompt=consent";

        return new GoogleOAuthRedirectUrl
        {
            RedirectUrl = googleAuthUrl,
            State = state,
        };
    }

    public async Task<GoogleToken> ExchangeCodeForTokenAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var redirectUri = $"{_appDomain}{_redirectPath}";

        var request = new
        {
            client_id = _clientId,
            client_secret = _clientSecret,
            code,
            grant_type = "authorization_code",
            redirect_uri = redirectUri,
        };

        var reponse = await _httpService.PostJsonAsync<GoogleToken>(
            "https://oauth2.googleapis.com/token",
            request,
            null,
            "Failed to exchange authorization code for access token",
            cancellationToken);

        return string.IsNullOrWhiteSpace(reponse.AccessToken)
            ? throw new ResultFailureException("Invalid response from Google token endpoint")
            : new GoogleToken
            {
                AccessToken = reponse.AccessToken,
                TokenType = reponse.TokenType,
                ExpiresIn = reponse.ExpiresIn,
                RefreshToken = reponse.RefreshToken,
            };
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var userInfo = await _httpService.GetAsync<GoogleUserInfo>(
            "https://www.googleapis.com/oauth2/v2/userinfo",
            accessToken,
            "Failed to retrieve user information from Google",
            cancellationToken);

        return string.IsNullOrWhiteSpace(userInfo.Email)
            ? throw new ResultFailureException("Invalid response from Google userinfo endpoint or email is missing")
            : new GoogleUserInfo
            {
                Id = userInfo.Id,
                Email = userInfo.Email,
                VerifiedEmail = userInfo.VerifiedEmail,
                Name = userInfo.Name,
                GivenName = userInfo.GivenName,
                FamilyName = userInfo.FamilyName,
                Picture = userInfo.Picture,
            };
    }

    private static string GenerateState()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }
}

