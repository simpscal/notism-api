using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Order.CreateOrder;
using Notism.Application.Order.GetOrderById;
using Notism.Application.Order.GetOrders;
using Notism.Application.Order.UpdateDeliveryStatus;

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

        group.MapGet("/{id:guid}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .WithSummary("Get order by ID")
            .WithDescription("Retrieves a specific order by ID for the authenticated user.")
            .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/delivery-status", UpdateDeliveryStatusAsync)
            .WithName("UpdateDeliveryStatus")
            .WithSummary("Update delivery status")
            .WithDescription("Updates the delivery status of an order.")
            .Produces<UpdateDeliveryStatusResponse>(StatusCodes.Status200OK)
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
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new GetOrderByIdRequest
        {
            OrderId = id,
            UserId = userId,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateDeliveryStatusAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        UpdateDeliveryStatusPayload payload,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new UpdateDeliveryStatusRequest
        {
            OrderId = id,
            UserId = userId,
            DeliveryStatus = payload.DeliveryStatus,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }
}

public record CreateOrderPayload
{
    public string PaymentMethod { get; set; } = string.Empty;
    public List<Guid> CartItemIds { get; set; } = new();
}

public record UpdateDeliveryStatusPayload
{
    public string DeliveryStatus { get; set; } = string.Empty;
}