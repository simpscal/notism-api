using System.Globalization;
using System.Text.Json;

using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Payment.CreateBankingCheckout;
using Notism.Application.Payment.GetBankAccount;
using Notism.Application.Payment.HandleSepayWebhook;
using Notism.Application.Payment.SaveBankAccount;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Shared.Extensions;

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
            .WithDescription("Retrieves the storer's configured bank account details. Admins call without parameters; consumers supply their checkoutId to prove an active checkout.")
            .Produces<GetBankAccountResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/bank-account", SaveBankAccountAsync)
            .WithName("SaveBankAccount")
            .WithSummary("Save bank account")
            .WithDescription("Creates or updates the caller's bank account. Admins upsert the store's inbound row; customers upsert their own refund-payout row. Owner type is derived from the role claim.")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapPost("/banking/checkout", CreateBankingCheckoutAsync)
            .WithName("CreateBankingCheckout")
            .WithSummary("Create banking checkout session")
            .WithDescription("Creates a BankingCheckout session for bank transfer payment. Returns a checkoutId used as the transfer reference.")
            .Produces<CreateBankingCheckoutResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        // Webhook group — no JWT auth; checkoutId+amount check limits blast radius
        var webhookGroup = app.MapGroup("/api/payments")
            .WithTags("Payment Management")
            .WithOpenApi()
            .AllowAnonymous();

        webhookGroup.MapPost("/webhook/sepay", HandleSepayWebhookAsync)
            .WithName("HandleSepayWebhook")
            .WithSummary("SePay webhook")
            .WithDescription("Receives SePay bank transfer notifications and auto-confirms matching orders.")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateBankingCheckoutAsync(
        HttpContext httpContext,
        IMediator mediator,
        CreateBankingCheckoutPayload payload,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new CreateBankingCheckoutRequest
        {
            UserId = userId,
            CartItemIds = payload.CartItemIds,
            TotalAmount = payload.TotalAmount,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
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

    private static async Task<IResult> HandleSepayWebhookAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(httpContext.Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        var payload = JsonSerializer.Deserialize<SepayWebhookPayload>(
            rawBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload is null)
        {
            return Results.Ok();
        }

        var localTime = DateTime.ParseExact(
            payload.TransactionDate,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture);
        var transferredAt = DateTime.SpecifyKind(localTime.AddHours(-7), DateTimeKind.Utc);

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = payload.TransactionId.ToString(CultureInfo.InvariantCulture),
            Amount = payload.Amount,
            Content = payload.Content,
            TransferType = payload.TransferType,
            TransferredAt = transferredAt,
        };

        await mediator.Send(request, CancellationToken.None);

        return Results.Ok();
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

    private static PaymentOwnerType ResolveOwnerType(HttpContext httpContext)
    {
        var role = httpContext.User.GetRole().FromCamelCase<UserRole>() ?? UserRole.User;

        return role == UserRole.Admin ? PaymentOwnerType.Store : PaymentOwnerType.Customer;
    }
}