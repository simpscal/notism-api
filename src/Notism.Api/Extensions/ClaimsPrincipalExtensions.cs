using System.Security.Claims;

using Notism.Shared.Exceptions;

namespace Notism.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the user ID from the JWT token's NameIdentifier claim.
    /// </summary>
    /// <param name="principal">The claims principal containing the JWT token claims.</param>
    /// <returns>The user ID as a Guid.</returns>
    /// <exception cref="ResultFailureException">Thrown when the user identifier is missing or invalid in the token.</exception>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId)
            ? throw new ResultFailureException("Invalid or missing user identifier in token")
            : userId;
    }

    /// <summary>
    /// Extracts the user role from the JWT token's Role claim.
    /// </summary>
    /// <param name="principal">The claims principal containing the JWT token claims.</param>
    /// <returns>The user role as a string (camelCase format).</returns>
    /// <exception cref="ResultFailureException">Thrown when the role is missing in the token.</exception>
    public static string GetRole(this ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

        return string.IsNullOrEmpty(roleClaim)
            ? throw new ResultFailureException("Invalid or missing role in token")
            : roleClaim;
    }
}