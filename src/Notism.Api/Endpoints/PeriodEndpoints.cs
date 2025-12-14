using MediatR;

using Microsoft.AspNetCore.Mvc;

using Notism.Api.Models;
using Notism.Application.Period.GetPeriods;

namespace Notism.Api.Endpoints;

public static class PeriodEndpoints
{
    public static void MapPeriodEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/periods")
            .WithTags("Periods")
            .WithOpenApi();

        group.MapGet("/", GetPeriodsAsync)
            .WithName("GetPeriods")
            .WithSummary("Get all published periods")
            .WithDescription("Retrieves all published periods in chronological order")
            .AllowAnonymous()
            .Produces<List<GetPeriodsResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetPeriodsAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var request = new GetPeriodsRequest();
        var response = await mediator.Send(request, cancellationToken);
        return Results.Ok(response);
    }
}

