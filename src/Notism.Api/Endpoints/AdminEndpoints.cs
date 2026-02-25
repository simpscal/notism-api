using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Food.DeleteFood;
using Notism.Application.Food.GetFoodById;
using Notism.Application.Food.GetFoods;
using Notism.Application.Food.UpdateFood;
using Notism.Application.Order.AdminOrdersForKanban;
using Notism.Application.Order.AdminOrdersForTable;
using Notism.Application.Order.AdminUpdateOrderDeliveryStatus;
using Notism.Application.Order.GetOrderById;
using Notism.Application.User.AdminDeleteUser;
using Notism.Application.User.AdminGetUsers;

namespace Notism.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        MapAdminUserEndpoints(app);
        MapAdminOrderEndpoints(app);
        MapAdminFoodEndpoints(app);
    }

    private static void MapAdminUserEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
            .WithTags("Admin User Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", AdminGetUsersAsync)
            .WithName("AdminGetUsers")
            .WithSummary("Get users")
            .WithDescription("Retrieves a paginated list of users with sorting and search support for admin portal.")
            .RequireAdmin()
            .Produces<AdminGetUsersResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", AdminDeleteUserAsync)
            .WithName("AdminDeleteUser")
            .WithSummary("Delete user")
            .WithDescription("Deletes a user by ID. Admins cannot delete themselves or other admins.")
            .RequireAdmin()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static void MapAdminOrderEndpoints(IEndpointRouteBuilder app)
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

        group.MapGet("/{slugId}", AdminGetOrderByIdAsync)
            .WithName("AdminGetOrderById")
            .WithSummary("Get order by slug ID")
            .WithDescription("Retrieves detailed information about a specific order by slug ID. Reuses GetOrderById with admin authorization.")
            .RequireAdmin()
            .Produces<GetOrderByIdResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

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

    private static void MapAdminFoodEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/foods")
            .WithTags("Admin Food Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", AdminGetFoodsAsync)
            .WithName("AdminGetFoods")
            .WithSummary("Get foods")
            .WithDescription("Retrieves a paginated list of foods with optional filtering and sorting for admin portal.")
            .RequireAdmin()
            .Produces<GetFoodsResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", AdminGetFoodByIdAsync)
            .WithName("AdminGetFoodById")
            .WithSummary("Get food by ID")
            .WithDescription("Retrieves detailed information about a specific food item.")
            .RequireAdmin()
            .Produces<GetFoodByIdResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}", AdminUpdateFoodAsync)
            .WithName("AdminUpdateFood")
            .WithSummary("Update food")
            .WithDescription("Updates an existing food item.")
            .RequireAdmin()
            .Produces<UpdateFoodResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteFoodAsync)
            .WithName("DeleteFood")
            .WithSummary("Delete food")
            .WithDescription("Deletes a food item by ID.")
            .RequireAdmin()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AdminGetUsersAsync(
        IMediator mediator,
        [AsParameters] AdminGetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminDeleteUserAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var callerUserId = httpContext.User.GetUserId();
        var request = new AdminDeleteUserRequest
        {
            TargetUserId = id,
            CallerUserId = callerUserId,
        };
        await mediator.Send(request, cancellationToken);
        return Results.NoContent();
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

    private static async Task<IResult> AdminGetOrderByIdAsync(
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

    private static async Task<IResult> AdminUpdateOrderDeliveryStatusAsync(
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

    private static async Task<IResult> AdminGetFoodsAsync(
        IMediator mediator,
        [AsParameters] GetFoodsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminGetFoodByIdAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new GetFoodByIdRequest { FoodId = id };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminUpdateFoodAsync(
        IMediator mediator,
        Guid id,
        UpdateFoodRequest request,
        CancellationToken cancellationToken)
    {
        request = request with { FoodId = id };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteFoodAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new DeleteFoodRequest { FoodId = id };
        await mediator.Send(request, cancellationToken);
        return Results.NoContent();
    }
}

public record AdminUpdateOrderDeliveryStatusPayload
{
    public string DeliveryStatus { get; set; } = string.Empty;
}
