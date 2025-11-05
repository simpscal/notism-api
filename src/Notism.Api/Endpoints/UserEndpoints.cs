using MediatR;

using Notism.Api.Attributes;
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

        group.MapGet("/{userId:guid}/profile", GetUserProfileAsync)
            .WithName("GetUserProfile")
            .WithSummary("Get user profile")
            .WithDescription("Retrieves a user's profile information.")
            .RequireAuthorization()
            .Produces<GetUserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/{userId:guid}/profile", UpdateUserProfileAsync)
            .WithName("UpdateUserProfile")
            .WithSummary("Update user profile (Admin only)")
            .WithDescription("Updates a user's profile information. Only administrators can perform this action.")
            .RequireAuthorization()
            .Produces<UpdateUserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetUserProfileAsync(
        Guid userId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new GetUserProfileRequest { UserId = userId };
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result.Value);
    }

    [RequireAdmin]
    private static async Task<IResult> UpdateUserProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        request.UserId = userId;

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result.Value);
    }
}