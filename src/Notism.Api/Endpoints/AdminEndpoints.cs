using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Food.AdminAddCategory;
using Notism.Application.Food.AdminAddCustomisationGroup;
using Notism.Application.Food.AdminAddCustomisationOption;
using Notism.Application.Food.AdminAddFood;
using Notism.Application.Food.AdminDeleteCategory;
using Notism.Application.Food.AdminDeleteCustomisationGroup;
using Notism.Application.Food.AdminDeleteCustomisationOption;
using Notism.Application.Food.AdminDeleteFood;
using Notism.Application.Food.AdminGetCategories;
using Notism.Application.Food.AdminGetCategoryDetail;
using Notism.Application.Food.AdminUpdateCategory;
using Notism.Application.Food.AdminUpdateCustomisationGroup;
using Notism.Application.Food.AdminUpdateCustomisationOption;
using Notism.Application.Food.AdminUpdateFood;
using Notism.Application.Food.GetFoodById;
using Notism.Application.Food.GetFoods;
using Notism.Application.Order.AdminGetOrderStatusSummary;
using Notism.Application.Order.AdminGetRevenueSeries;
using Notism.Application.Order.AdminGetTodaySales;
using Notism.Application.Order.AdminOrdersForKanban;
using Notism.Application.Order.AdminOrdersForTable;
using Notism.Application.Order.AdminRefundsForTable;
using Notism.Application.Order.AdminUpdateOrderDeliveryStatus;
using Notism.Application.Order.AdminUpdateOrderPaymentStatus;
using Notism.Application.Order.ApproveRefund;
using Notism.Application.Order.Common;
using Notism.Application.Order.GetOrderById;
using Notism.Application.Order.MarkRefundFailed;
using Notism.Application.Order.RetryRefund;
using Notism.Application.User.AdminDeleteUser;
using Notism.Application.User.AdminGetUserDetail;
using Notism.Application.User.AdminGetUsers;
using Notism.Application.User.AdminUpdateUser;

namespace Notism.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        MapAdminUserEndpoints(app);
        MapAdminOrderEndpoints(app);
        MapAdminRefundEndpoints(app);
        MapAdminDashboardEndpoints(app);
        MapAdminCategoryEndpoints(app);
        MapAdminFoodEndpoints(app);
        MapAdminFoodCustomisationEndpoints(app);
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

        group.MapGet("/{id:guid}", AdminGetUserDetailAsync)
            .WithName("AdminGetUserDetail")
            .WithSummary("Get user detail")
            .WithDescription("Retrieves detailed information about a specific user by ID.")
            .RequireAdmin()
            .Produces<AdminUserDetailResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}", AdminUpdateUserAsync)
            .WithName("AdminUpdateUser")
            .WithSummary("Update user detail")
            .WithDescription("Updates a user's detail. Returns the same shape as Get user detail.")
            .RequireAdmin()
            .Produces<AdminUserDetailResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

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

        group.MapPatch("/{id:guid}/payment-status", AdminUpdateOrderPaymentStatusAsync)
            .WithName("AdminUpdateOrderPaymentStatus")
            .WithSummary("Update payment status")
            .WithDescription("Updates the payment status of an order.")
            .RequireAdmin()
            .Produces<AdminUpdateOrderPaymentStatusResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static void MapAdminRefundEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/refunds")
            .WithTags("Admin Refund Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", AdminRefundsForTableAsync)
            .WithName("AdminRefundsForTable")
            .WithSummary("Get refunds ledger")
            .WithDescription("Retrieves a paginated list of refunds with optional status filtering for the reconcile ledger. The unfiltered list exposes every admin status, including processing and failed.")
            .RequireAdmin()
            .Produces<AdminRefundsForTableResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapPost("/{id:guid}/approve", AdminApproveRefundAsync)
            .WithName("AdminApproveRefund")
            .WithSummary("Approve refund")
            .WithDescription("Transitions a pending refund to processing. Staff transfer funds manually; no provider call is made.")
            .RequireAdmin()
            .Produces<RefundResponse>(StatusCodes.Status202Accepted)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/retry", AdminRetryRefundAsync)
            .WithName("AdminRetryRefund")
            .WithSummary("Retry refund")
            .WithDescription("Transitions a failed refund back to processing. Staff transfer funds manually; no provider call is made.")
            .RequireAdmin()
            .Produces<RefundResponse>(StatusCodes.Status202Accepted)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/mark-failed", AdminMarkRefundFailedAsync)
            .WithName("AdminMarkRefundFailed")
            .WithSummary("Mark refund failed")
            .WithDescription("Marks a processing refund as failed with a reason.")
            .RequireAdmin()
            .Produces<RefundResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict);
    }

    private static void MapAdminDashboardEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/dashboard")
            .WithTags("Admin Dashboard")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/order-status-summary", AdminGetOrderStatusSummaryAsync)
            .WithName("AdminGetOrderStatusSummary")
            .WithSummary("Get order status summary")
            .WithDescription("Retrieves order counts grouped into the dashboard delivery-status buckets (new, in progress, completed). Cancelled orders are excluded and every bucket is always present.")
            .RequireAdmin()
            .Produces<AdminGetOrderStatusSummaryResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/today-sales", AdminGetTodaySalesAsync)
            .WithName("AdminGetTodaySales")
            .WithSummary("Get today's sales")
            .WithDescription("Retrieves headline sales figures (total revenue and order count) for the client-supplied UTC window [startUtc, endUtc). Revenue sums paid orders; order count counts orders created in the window. Both are zero when there is no matching activity. The server is time-zone agnostic and derives no window.")
            .RequireAdmin()
            .Produces<AdminGetTodaySalesResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/revenue-series", AdminGetRevenueSeriesAsync)
            .WithName("AdminGetRevenueSeries")
            .WithSummary("Get revenue series")
            .WithDescription("Retrieves an ordered, dense per-bucket revenue series. The client supplies the n+1 ascending UTC boundaries and one label per bucket; revenue is the SUM of paid orders whose PaidAt falls in each half-open [boundaries[i], boundaries[i+1]) range. Every bucket is present, with zero-revenue buckets reported as 0. Granularity is an optional client hint echoed back verbatim and carries no server semantics.")
            .RequireAdmin()
            .Produces<AdminGetRevenueSeriesResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);
    }

    private static void MapAdminCategoryEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/categories")
            .WithTags("Admin Category Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", AdminGetCategoriesAsync)
            .WithName("AdminGetCategories")
            .WithSummary("Get categories")
            .WithDescription("Retrieves all categories, excluding deleted ones.")
            .RequireAdmin()
            .Produces<AdminGetCategoriesResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", AdminGetCategoryDetailAsync)
            .WithName("AdminGetCategoryDetail")
            .WithSummary("Get category detail")
            .WithDescription("Retrieves a single category by ID for the admin portal. Excludes deleted categories.")
            .RequireAdmin()
            .Produces<AdminGetCategoryDetailResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/", AdminAddCategoryAsync)
            .WithName("AdminAddCategory")
            .WithSummary("Add category")
            .WithDescription("Creates a new category. Validates name uniqueness.")
            .RequireAdmin()
            .Produces<AdminAddCategoryResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapPatch("/{id:guid}", AdminUpdateCategoryAsync)
            .WithName("AdminUpdateCategory")
            .WithSummary("Update category")
            .WithDescription("Updates a category. Validates name uniqueness excluding the current name.")
            .RequireAdmin()
            .Produces<AdminUpdateCategoryResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", AdminDeleteCategoryAsync)
            .WithName("AdminDeleteCategory")
            .WithSummary("Delete category")
            .WithDescription("Soft deletes a category by ID.")
            .RequireAdmin()
            .Produces(StatusCodes.Status204NoContent)
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

        group.MapPost("/", AdminAddFoodAsync)
            .WithName("AdminAddFood")
            .WithSummary("Add food")
            .WithDescription("Creates a new food item.")
            .RequireAdmin()
            .Produces<AdminAddFoodResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden);

        group.MapPatch("/{id:guid}", AdminUpdateFoodAsync)
            .WithName("AdminUpdateFood")
            .WithSummary("Update food")
            .WithDescription("Updates an existing food item.")
            .RequireAdmin()
            .Produces<AdminUpdateFoodResponse>(StatusCodes.Status200OK)
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

    private static void MapAdminFoodCustomisationEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/foods/{foodId:guid}/customisation-groups")
            .WithTags("Admin Food Customisation Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", AdminAddCustomisationGroupAsync)
            .WithName("AdminAddCustomisationGroup")
            .WithSummary("Add customisation group")
            .WithDescription("Adds a new customisation group to a food item.")
            .RequireAdmin()
            .Produces<AdminAddCustomisationGroupResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPatch("/{groupId:guid}", AdminUpdateCustomisationGroupAsync)
            .WithName("AdminUpdateCustomisationGroup")
            .WithSummary("Update customisation group")
            .WithDescription("Updates an existing customisation group on a food item.")
            .RequireAdmin()
            .Produces<AdminUpdateCustomisationGroupResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{groupId:guid}", AdminDeleteCustomisationGroupAsync)
            .WithName("AdminDeleteCustomisationGroup")
            .WithSummary("Delete customisation group")
            .WithDescription("Deletes a customisation group and all its options from a food item.")
            .RequireAdmin()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/{groupId:guid}/options", AdminAddCustomisationOptionAsync)
            .WithName("AdminAddCustomisationOption")
            .WithSummary("Add customisation option")
            .WithDescription("Adds a new option to a customisation group.")
            .RequireAdmin()
            .Produces<AdminAddCustomisationOptionResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPatch("/{groupId:guid}/options/{optionId:guid}", AdminUpdateCustomisationOptionAsync)
            .WithName("AdminUpdateCustomisationOption")
            .WithSummary("Update customisation option")
            .WithDescription("Updates an existing option within a customisation group.")
            .RequireAdmin()
            .Produces<AdminUpdateCustomisationOptionResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/{groupId:guid}/options/{optionId:guid}", AdminDeleteCustomisationOptionAsync)
            .WithName("AdminDeleteCustomisationOption")
            .WithSummary("Delete customisation option")
            .WithDescription("Deletes an option from a customisation group.")
            .RequireAdmin()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> AdminAddCustomisationGroupAsync(
        IMediator mediator,
        Guid foodId,
        AdminAddCustomisationGroupPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new AdminAddCustomisationGroupRequest
        {
            FoodId = foodId,
            Label = payload.Label,
            IsRequired = payload.IsRequired,
            DisplayOrder = payload.DisplayOrder,
        };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Created(
            $"/api/admin/foods/{foodId}/customisation-groups/{result.Id}",
            result);
    }

    private static async Task<IResult> AdminUpdateCustomisationGroupAsync(
        IMediator mediator,
        Guid foodId,
        Guid groupId,
        AdminUpdateCustomisationGroupPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new AdminUpdateCustomisationGroupRequest
        {
            FoodId = foodId,
            GroupId = groupId,
            Label = payload.Label,
            IsRequired = payload.IsRequired,
            DisplayOrder = payload.DisplayOrder,
        };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminDeleteCustomisationGroupAsync(
        IMediator mediator,
        Guid foodId,
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var request = new AdminDeleteCustomisationGroupRequest
        {
            FoodId = foodId,
            GroupId = groupId,
        };
        await mediator.Send(request, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AdminAddCustomisationOptionAsync(
        IMediator mediator,
        Guid foodId,
        Guid groupId,
        AdminAddCustomisationOptionPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new AdminAddCustomisationOptionRequest
        {
            FoodId = foodId,
            GroupId = groupId,
            Label = payload.Label,
            Surcharge = payload.Surcharge,
            DisplayOrder = payload.DisplayOrder,
        };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Created(
            $"/api/admin/foods/{foodId}/customisation-groups/{groupId}/options/{result.Id}",
            result);
    }

    private static async Task<IResult> AdminUpdateCustomisationOptionAsync(
        IMediator mediator,
        Guid foodId,
        Guid groupId,
        Guid optionId,
        AdminUpdateCustomisationOptionPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new AdminUpdateCustomisationOptionRequest
        {
            FoodId = foodId,
            GroupId = groupId,
            OptionId = optionId,
            Label = payload.Label,
            Surcharge = payload.Surcharge,
            DisplayOrder = payload.DisplayOrder,
        };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminDeleteCustomisationOptionAsync(
        IMediator mediator,
        Guid foodId,
        Guid groupId,
        Guid optionId,
        CancellationToken cancellationToken)
    {
        var request = new AdminDeleteCustomisationOptionRequest
        {
            FoodId = foodId,
            GroupId = groupId,
            OptionId = optionId,
        };
        await mediator.Send(request, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AdminGetCategoriesAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new AdminGetCategoriesRequest();
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminGetCategoryDetailAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new AdminGetCategoryDetailRequest { CategoryId = id };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminAddCategoryAsync(
        IMediator mediator,
        AdminAddCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Results.Created($"/api/admin/categories/{result.Id}", result);
    }

    private static async Task<IResult> AdminUpdateCategoryAsync(
        IMediator mediator,
        Guid id,
        AdminUpdateCategoryPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new AdminUpdateCategoryRequest
        {
            CategoryId = id,
            Name = payload.Name,
        };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminDeleteCategoryAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new AdminDeleteCategoryRequest { CategoryId = id };
        await mediator.Send(request, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AdminGetUsersAsync(
        IMediator mediator,
        [AsParameters] AdminGetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminGetUserDetailAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new AdminGetUserDetailRequest { UserId = id };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminUpdateUserAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        AdminUpdateUserPayload payload,
        CancellationToken cancellationToken)
    {
        var callerUserId = httpContext.User.GetUserId();
        var request = new AdminUpdateUserRequest
        {
            TargetUserId = id,
            CallerUserId = callerUserId,
            Role = payload.Role,
        };
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

    private static async Task<IResult> AdminGetOrderStatusSummaryAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new AdminGetOrderStatusSummaryRequest();
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminGetTodaySalesAsync(
        IMediator mediator,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        var request = new AdminGetTodaySalesRequest
        {
            StartUtc = startUtc,
            EndUtc = endUtc,
        };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminGetRevenueSeriesAsync(
        IMediator mediator,
        DateTime[] boundaries,
        string[] labels,
        string? granularity,
        CancellationToken cancellationToken)
    {
        var request = new AdminGetRevenueSeriesRequest
        {
            Boundaries = boundaries.ToList(),
            Labels = labels.ToList(),
            Granularity = granularity,
        };
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

    private static async Task<IResult> AdminUpdateOrderPaymentStatusAsync(
        IMediator mediator,
        Guid id,
        AdminUpdateOrderPaymentStatusPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new AdminUpdateOrderPaymentStatusRequest
        {
            OrderId = id,
            PaymentStatus = payload.PaymentStatus,
        };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminRefundsForTableAsync(
        IMediator mediator,
        [AsParameters] AdminRefundsForTableRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminApproveRefundAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new ApproveRefundRequest { RefundId = id };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Accepted(value: result);
    }

    private static async Task<IResult> AdminRetryRefundAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new RetryRefundRequest { RefundId = id };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Accepted(value: result);
    }

    private static async Task<IResult> AdminMarkRefundFailedAsync(
        IMediator mediator,
        Guid id,
        MarkRefundFailedPayload payload,
        CancellationToken cancellationToken)
    {
        var request = new MarkRefundFailedRequest
        {
            RefundId = id,
            Reason = payload.Reason,
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
        var request = new GetFoodByIdRequest { FoodId = id, IncludeEmptyGroups = true };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> AdminAddFoodAsync(
        IMediator mediator,
        AdminAddFoodRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Results.Created($"/api/admin/foods/{result.Id}", result);
    }

    private static async Task<IResult> AdminUpdateFoodAsync(
        IMediator mediator,
        Guid id,
        AdminUpdateFoodRequest request,
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
        var request = new AdminDeleteFoodRequest { FoodId = id };
        await mediator.Send(request, cancellationToken);
        return Results.NoContent();
    }
}