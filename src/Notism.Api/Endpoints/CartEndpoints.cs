using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Models;
using Notism.Application.Cart.AddCartItem;
using Notism.Application.Cart.ClearCart;
using Notism.Application.Cart.GetCartItems;
using Notism.Application.Cart.RemoveCartItem;
using Notism.Application.Cart.UpdateCartItemQuantity;

namespace Notism.Api.Endpoints;

public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart")
            .WithTags("Cart Management")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetCartItemsAsync)
            .WithName("GetCartItems")
            .WithSummary("Get cart items")
            .WithDescription("Retrieves all cart items for the authenticated user.")
            .Produces<GetCartItemsResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapPost("/items", AddCartItemAsync)
            .WithName("AddCartItem")
            .WithSummary("Add item to cart")
            .WithDescription("Adds a food item to the user's cart.")
            .Produces<AddCartItemResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        group.MapPatch("/items/{id:guid}", UpdateCartItemQuantityAsync)
            .WithName("UpdateCartItemQuantity")
            .WithSummary("Update cart item quantity")
            .WithDescription("Updates the quantity of a specific cart item.")
            .Produces<UpdateCartItemQuantityResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/items/{id:guid}", RemoveCartItemAsync)
            .WithName("RemoveCartItem")
            .WithSummary("Remove item from cart")
            .WithDescription("Removes a specific item from the user's cart.")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapDelete("/", ClearCartAsync)
            .WithName("ClearCart")
            .WithSummary("Clear cart")
            .WithDescription("Removes all items from the user's cart.")
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetCartItemsAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new GetCartItemsRequest { UserId = userId };
        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> AddCartItemAsync(
        HttpContext httpContext,
        IMediator mediator,
        AddCartItemPayload payload,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new AddCartItemRequest
        {
            UserId = userId,
            FoodId = payload.FoodId,
            Quantity = payload.Quantity,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateCartItemQuantityAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        UpdateCartItemQuantityPayload payloda,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new UpdateCartItemQuantityRequest
        {
            UserId = userId,
            CartItemId = id,
            Quantity = payloda.Quantity,
        };

        var result = await mediator.Send(request, cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> RemoveCartItemAsync(
        HttpContext httpContext,
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new RemoveCartItemRequest
        {
            UserId = userId,
            CartItemId = id,
        };

        await mediator.Send(request, cancellationToken);

        return Results.Ok();
    }

    private static async Task<IResult> ClearCartAsync(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = httpContext.User.GetUserId();

        var request = new ClearCartRequest { UserId = userId };
        await mediator.Send(request, cancellationToken);

        return Results.Ok();
    }
}

public record AddCartItemPayload
{
    public Guid FoodId { get; set; }
    public int Quantity { get; set; }
}

public record UpdateCartItemQuantityPayload
{
    public int Quantity { get; set; }
}