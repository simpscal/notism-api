using System.Globalization;
using System.Text.Json;

using MediatR;

using Notism.Application.Order.HandleSepayWebhook;

namespace Notism.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
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
}
