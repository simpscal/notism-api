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
}