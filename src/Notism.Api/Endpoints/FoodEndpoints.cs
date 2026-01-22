using MediatR;

using Notism.Api.Models;
using Notism.Application.Food.GetFoodById;
using Notism.Application.Food.GetFoods;

namespace Notism.Api.Endpoints;

public static class FoodEndpoints
{
    public static void MapFoodEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/foods")
            .WithTags("Food Management")
            .WithOpenApi();

        group.MapPost("/", GetFoodsAsync)
            .WithName("GetFoods")
            .WithSummary("Get list of foods")
            .WithDescription("Retrieves a paginated list of foods with optional filtering by category and search term.")
            .Produces<GetFoodsResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetFoodByIdAsync)
            .WithName("GetFoodById")
            .WithSummary("Get food by ID")
            .WithDescription("Retrieves detailed information about a specific food item.")
            .Produces<GetFoodByIdResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetFoodsAsync(
        IMediator mediator,
        GetFoodsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetFoodByIdAsync(
        IMediator mediator,
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new GetFoodByIdRequest { FoodId = id };
        var result = await mediator.Send(request, cancellationToken);
        return Results.Ok(result);
    }
}