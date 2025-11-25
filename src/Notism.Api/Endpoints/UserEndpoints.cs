using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.User.GetProfile;
using Notism.Application.User.UpdateProfile;

namespace Notism.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("User Management")
            .WithOpenApi();

        group.MapGet("/profile", GetUserProfileAsync)
            .WithName("GetUserProfile")
            .WithSummary("Get user profile")
            .WithDescription("Retrieves the authenticated user's profile information.")
            .RequireAuthorization()
            .Produces<GetUserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/profile", UpdateUserProfileAsync)
            .WithName("UpdateUserProfile")
            .WithSummary("Update user profile")
            .WithDescription("Updates the authenticated user's profile information.")
            .RequireAuthorization()
            .Produces<UpdateUserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetUserProfileAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new GetUserProfileRequest { UserId = userId };
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateUserProfileAsync(
        HttpContext httpContext,
        UpdateUserProfileRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();
        request.UserId = userId;

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }
}