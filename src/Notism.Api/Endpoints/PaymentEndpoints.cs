using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Payment.GetBankAccount;
using Notism.Application.Payment.SaveBankAccount;

namespace Notism.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payment Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/bank-account", GetBankAccountAsync)
            .WithName("GetBankAccount")
            .WithSummary("Get bank account")
            .WithDescription("Retrieves the storer's configured bank account details.")
            .RequireAdmin()
            .Produces<GetBankAccountResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapPut("/bank-account", SaveBankAccountAsync)
            .WithName("SaveBankAccount")
            .WithSummary("Save bank account")
            .WithDescription("Creates or updates the storer's bank account details.")
            .RequireAdmin()
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
        var userId = httpContext.User.GetUserId();

        var request = new GetBankAccountRequest { StorerId = userId };
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> SaveBankAccountAsync(
        HttpContext httpContext,
        IMediator mediator,
        SaveBankAccountPayload payload,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new SaveBankAccountRequest
        {
            StorerId = userId,
            BankCode = payload.BankCode,
            AccountNumber = payload.AccountNumber,
            AccountHolderName = payload.AccountHolderName,
        };

        await mediator.Send(request, cancellationToken);

        return Results.Ok();
    }
}

public record SaveBankAccountPayload
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}
