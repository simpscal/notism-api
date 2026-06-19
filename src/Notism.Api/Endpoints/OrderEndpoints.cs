using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Order.CancelOrder;
using Notism.Application.Order.Common;
using Notism.Application.Order.CreateBankingCheckout;
using Notism.Application.Order.CreateOrder;
using Notism.Application.Order.GetHeldRefunds;
using Notism.Application.Order.GetOrderById;
using Notism.Application.Order.GetOrders;
using Notism.Application.Order.RequestRefund;

namespace Notism.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Order Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", CreateOrderAsync)
            .WithName("CreateOrder")
            .WithSummary("Create order")
            .WithDescription("Creates a new order from the user's cart items. Cart items are removed after order creation.")
            .Produces<CreateOrderResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/", GetOrdersAsync)
            .WithName("GetOrders")
            .WithSummary("Get orders")
            .WithDescription("Retrieves a page of orders for the authenticated user, most recent first.")
            .Produces<GetOrdersResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/held-refunds", GetHeldRefundsAsync)
            .WithName("GetHeldRefunds")
            .WithSummary("Get held refunds awaiting bank details")
            .WithDescription("Retrieves the authenticated customer's refunds held awaiting bank details. Returns an empty array once bank details are on file.")
            .Produces<HeldRefundResponse[]>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapGet("/{slugId}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .WithSummary("Get order by slug ID")
            .WithDescription("Retrieves a specific order by slug ID for the authenticated user.")
            .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/cancel", CancelOrderAsync)
            .WithName("CancelOrder")
            .WithSummary("Cancel order")
            .WithDescription("Cancels an order. The order can only be cancelled if the delivery status is OrderPlaced or Preparing.")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/refund", RequestRefundAsync)
            .WithName("RequestRefund")
            .WithSummary("Request refund")
            .WithDescription("Requests a refund for a delivered bank-transfer order within 24 hours of delivery. A pending refund for the full order total is created.")
            .Produces<OrderRefundResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/banking/checkout", CreateBankingCheckoutAsync)
            .WithName("CreateBankingCheckout")
            .WithSummary("Create banking checkout session")
            .WithDescription("Creates a BankingCheckout session for bank transfer payment. Returns a checkoutId used as the transfer reference.")
            .Produces<CreateBankingCheckoutResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);
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

    private static async Task<IResult> CreateOrderAsync(
        HttpContext httpContext,
        IMediator mediator,
        CreateOrderPayload payload,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new CreateOrderRequest
        {
            UserId = userId,
            PaymentMethod = payload.PaymentMethod,
            CartItemIds = payload.CartItemIds,
            DeliveryNotes = payload.DeliveryNotes,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetOrdersAsync(
        HttpContext httpContext,
        IMediator mediator,
        string? paymentStatus,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 25)
    {
        var userId = httpContext.User.GetUserId();

        var request = new GetOrdersRequest
        {
            UserId = userId,
            PaymentStatus = paymentStatus,
            Skip = skip,
            Take = take,
        };
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetHeldRefundsAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new GetHeldRefundsRequest
        {
            UserId = userId,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result.Items);
    }

    private static async Task<IResult> GetOrderByIdAsync(
        HttpContext httpContext,
        IMediator mediator,
        string slugId,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();
        var role = httpContext.User.GetRole();

        var request = new GetOrderByIdRequest
        {
            SlugId = slugId,
            UserId = userId,
            Role = role,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CancelOrderAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new CancelOrderRequest
        {
            OrderId = id,
            UserId = userId,
        };

        await mediator.Send(request, cancellationToken);

        return Results.Ok();
    }

    private static async Task<IResult> RequestRefundAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new RequestRefundRequest
        {
            OrderId = id,
            UserId = userId,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }
}