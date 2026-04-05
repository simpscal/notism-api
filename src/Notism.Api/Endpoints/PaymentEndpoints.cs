using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using MediatR;

using Microsoft.Extensions.Options;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Payment.GetBankAccount;
using Notism.Application.Payment.HandleSepayWebhook;
using Notism.Application.Payment.SaveBankAccount;
using Notism.Shared.Configuration;

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

        // Webhook group — no JWT auth, verified via HMAC
        var webhookGroup = app.MapGroup("/api/payments")
            .WithTags("Payment Management")
            .WithOpenApi()
            .AllowAnonymous();

        webhookGroup.MapPost("/webhook/sepay", HandleSepayWebhookAsync)
            .WithName("HandleSepayWebhook")
            .WithSummary("SePay webhook")
            .WithDescription("Receives SePay bank transfer notifications and auto-confirms matching orders.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
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
        IOptions<SepaySettings> sepayOptions,
        CancellationToken cancellationToken)
    {
        httpContext.Request.EnableBuffering();

        using var reader = new StreamReader(httpContext.Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        httpContext.Request.Body.Position = 0;

        var incomingSignature = httpContext.Request.Headers["X-Sepay-Signature"].FirstOrDefault();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sepayOptions.Value.WebhookSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
        var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();

        if (!string.Equals(incomingSignature, computedSignature, StringComparison.OrdinalIgnoreCase))
        {
            return Results.Unauthorized();
        }

        var payload = JsonSerializer.Deserialize<SepayWebhookPayload>(
            rawBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload is null)
        {
            return Results.Ok();
        }

        var request = new HandleSepayWebhookRequest
        {
            TransactionId = payload.TransactionId,
            Amount = payload.Amount,
            Description = payload.Description,
            TransferredAt = payload.TransferredAt,
        };

        await mediator.Send(request, cancellationToken);

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

public record SaveBankAccountPayload
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}

public record SepayWebhookPayload
{
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime TransferredAt { get; init; }
}
