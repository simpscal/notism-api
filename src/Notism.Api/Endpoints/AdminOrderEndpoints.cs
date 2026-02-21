using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Order.AdminOrdersForKanban;
using Notism.Application.Order.AdminOrdersForTable;
using Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

namespace Notism.Api.Endpoints;

public static class AdminOrderEndpoints
{
    public static void MapAdminOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/orders")
            .WithTags("Admin Order Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/kanban", AdminOrdersForKanbanAsync)
            .WithName("AdminOrdersForKanban")
            .WithSummary("Get orders for kanban view")
            .WithDescription("Retrieves a paginated list of orders filtered by delivery status for kanban view.")
            .RequireAdmin()
            .Produces<AdminOrdersForKanbanResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/table", AdminOrdersForTableAsync)
            .WithName("AdminOrdersForTable")
            .WithSummary("Get orders for table view")
            .WithDescription("Retrieves a paginated list of orders with sorting and search support for table view.")
            .RequireAdmin()
            .Produces<AdminOrdersForTableResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapPatch("/{id:guid}/delivery-status", AdminUpdateOrderDeliveryStatusAsync)
            .WithName("AdminUpdateOrderDeliveryStatus")
            .WithSummary("Update delivery status")
            .WithDescription("Updates the delivery status of an order.")
            .RequireAdmin()
            .Produces<AdminUpdateOrderDeliveryStatusResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AdminOrdersForKanbanAsync(
        IMediator mediator,
        [AsParameters] AdminOrdersForKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> AdminOrdersForTableAsync(
        IMediator mediator,
        [AsParameters] AdminOrdersForTableRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> AdminUpdateOrderDeliveryStatusAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        AdminUpdateOrderDeliveryStatusPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new AdminUpdateOrderDeliveryStatusRequest
        {
            OrderId = id,
            DeliveryStatus = payload.DeliveryStatus,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }
}

public record AdminUpdateOrderDeliveryStatusPayload
{
    public string DeliveryStatus { get; set; } = string.Empty;
}