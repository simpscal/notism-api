using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.User.GetBankAccount;
using Notism.Application.User.GetProfile;
using Notism.Application.User.SaveBankAccount;
using Notism.Application.User.UpdateProfile;
using Notism.Domain.User.Enums;
using Notism.Shared.Extensions;

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

        group.MapGet("/bank-account", GetBankAccountAsync)
            .WithName("GetBankAccount")
            .WithSummary("Get bank account")
            .WithDescription("Retrieves the storer's configured bank account details. Admins call without parameters; consumers supply their checkoutId to prove an active checkout.")
            .RequireAuthorization()
            .Produces<GetBankAccountResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/bank-account", SaveBankAccountAsync)
            .WithName("SaveBankAccount")
            .WithSummary("Save bank account")
            .WithDescription("Creates or updates the caller's bank account. Admins upsert the store's inbound row; customers upsert their own refund-payout row. Owner type is derived from the role claim.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetBankAccountAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new GetBankAccountRequest
        {
            OwnerId = httpContext.User.GetUserId(),
            OwnerType = ResolveOwnerType(httpContext),
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> SaveBankAccountAsync(
        HttpContext httpContext,
        IMediator mediator,
        SaveBankAccountPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new SaveBankAccountRequest
        {
            OwnerId = httpContext.User.GetUserId(),
            OwnerType = ResolveOwnerType(httpContext),
            BankCode = payload.BankCode,
            AccountNumber = payload.AccountNumber,
            AccountHolderName = payload.AccountHolderName,
        };

        await mediator.Send(request, cancellationToken);

        return Results.Ok();
    }

    private static BankAccountOwnerType ResolveOwnerType(HttpContext httpContext)
    {
        var role = httpContext.User.GetRole().FromCamelCase<UserRole>() ?? UserRole.User;

        return role == UserRole.Admin ? BankAccountOwnerType.Store : BankAccountOwnerType.Customer;
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