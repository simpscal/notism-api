using Microsoft.AspNetCore.Builder;

namespace Notism.Api.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder RequireAdmin(this RouteHandlerBuilder builder)
    {
        return builder.RequireAuthorization(policy => policy.RequireRole("admin"));
    }
}