using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Payment.CreateBankingCheckout;
using Notism.Application.Payment.GetBankAccount;
using Notism.Application.Payment.HandleSepayWebhook;
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
        var userId = httpContext.User.GetUserId();

        var request = new GetBankAccountRequest { StorerId = userId };
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

public record CreateBankingCheckoutPayload
{
    public List<Guid> CartItemIds { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public record SaveBankAccountPayload
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}

public record SepayWebhookPayload
{
    [JsonPropertyName("id")]
    public long TransactionId { get; init; }

    [JsonPropertyName("transferAmount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("transactionDate")]
    public string TransactionDate { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}
