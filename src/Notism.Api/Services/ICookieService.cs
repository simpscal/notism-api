namespace Notism.Api.Services;

public interface ICookieService
{
    void SetRefreshTokenCookie(HttpContext context, string refreshToken, DateTime expiresAt);
    string? GetRefreshTokenFromCookie(HttpContext context);
    void ClearRefreshTokenCookie(HttpContext context);

    Task<string> GenerateAntiForgeryTokenAsync(HttpContext context);
    Task ValidateAntiForgeryTokenAsync(HttpContext context);
}
