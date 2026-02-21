using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.User.AdminDeleteUser;
using Notism.Application.User.AdminGetUsers;

namespace Notism.Api.Endpoints;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin User Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", AdminGetUsersAsync)
            .WithName("AdminGetUsers")
            .WithSummary("Get users")
            .WithDescription("Retrieves a paginated list of users with sorting and search support for admin portal.")
            .RequireAdmin()
            .Produces<AdminGetUsersResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", AdminDeleteUserAsync)
            .WithName("AdminDeleteUser")
            .WithSummary("Delete user")
            .WithDescription("Deletes a user by ID. Admins cannot delete themselves or other admins.")
            .RequireAdmin()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AdminGetUsersAsync(
        IMediator mediator,
        [AsParameters] AdminGetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> AdminDeleteUserAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var callerUserId = httpContext.User.GetUserId();

        var request = new AdminDeleteUserRequest
        {
            TargetUserId = id,
            CallerUserId = callerUserId,
        };

        await mediator.Send(request, cancellationToken);

        return Results.NoContent();
    }
}