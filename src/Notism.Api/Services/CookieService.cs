using Microsoft.AspNetCore.Antiforgery;

namespace Notism.Api.Services;

public class CookieService : ICookieService
{
    private const string RefreshTokenCookieName = "X-Refresh-Token";
    private const string AntiForgeryTokenHeaderName = "X-XSRF-TOKEN";
    private readonly IAntiforgery _antiforgery;
    private readonly IWebHostEnvironment _environment;

    public CookieService(IAntiforgery antiforgery, IWebHostEnvironment environment)
    {
        _antiforgery = antiforgery;
        _environment = environment;
    }

    public void SetRefreshTokenCookie(HttpContext context, string refreshToken, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/",
        };

        context.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    public string? GetRefreshTokenFromCookie(HttpContext context)
    {
        return context.Request.Cookies[RefreshTokenCookieName];
    }

    public void ClearRefreshTokenCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/",
        });
    }

    public Task<string> GenerateAntiForgeryTokenAsync(HttpContext context)
    {
        var tokens = _antiforgery.GetAndStoreTokens(context);
        context.Response.Headers.Append(AntiForgeryTokenHeaderName, tokens.RequestToken!);
        return Task.FromResult(tokens.RequestToken!);
    }

    public Task ValidateAntiForgeryTokenAsync(HttpContext context)
    {
        return _antiforgery.ValidateRequestAsync(context);
    }
}
