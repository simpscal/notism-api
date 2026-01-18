using Microsoft.AspNetCore.Antiforgery;

using Notism.Shared.Constants;
using Notism.Shared.Exceptions;

namespace Notism.Api.Services;

public class CookieService : ICookieService
{
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
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = expiresAt,
            Path = "/",
        };

        context.Response.Cookies.Append(CookieNames.RefreshToken, refreshToken, cookieOptions);
    }

    public string? GetRefreshTokenFromCookie(HttpContext context)
    {
        return context.Request.Cookies[CookieNames.RefreshToken];
    }

    public void ClearAuthenticationCookies(HttpContext context)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
        };

        context.Response.Cookies.Delete(CookieNames.RefreshToken, cookieOptions);
        context.Response.Cookies.Delete(CookieNames.AntiForgery, cookieOptions);
        context.Response.Headers.Remove(HeaderNames.AntiForgeryToken);
    }

    public Task<string> GenerateAntiForgeryTokenAsync(HttpContext context)
    {
        var tokens = _antiforgery.GetAndStoreTokens(context);
        context.Response.Headers.Append(HeaderNames.AntiForgeryToken, tokens.RequestToken!);
        return Task.FromResult(tokens.RequestToken!);
    }

    public async Task ValidateAntiForgeryTokenAsync(HttpContext context)
    {
        try
        {
            await _antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            throw new ResultFailureException("Invalid or missing antiforgery token");
        }
    }
}