using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Order.CancelOrder;
using Notism.Application.Order.CreateOrder;
using Notism.Application.Order.GetOrderById;
using Notism.Application.Order.GetOrders;

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
            .WithDescription("Retrieves all orders for the authenticated user.")
            .Produces<GetOrdersResponse>(StatusCodes.Status200OK)
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
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetOrdersAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new GetOrdersRequest { UserId = userId };
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
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
}

public record CreateOrderPayload
{
    public string PaymentMethod { get; set; } = string.Empty;
    public List<Guid> CartItemIds { get; set; } = new();
}