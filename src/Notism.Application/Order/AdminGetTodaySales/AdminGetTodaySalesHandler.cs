using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;

namespace Notism.Application.Order.AdminGetTodaySales;

public class AdminGetTodaySalesHandler
    : IRequestHandler<AdminGetTodaySalesRequest, AdminGetTodaySalesResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetTodaySalesHandler> _logger;

    public AdminGetTodaySalesHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetTodaySalesHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminGetTodaySalesResponse> Handle(
        AdminGetTodaySalesRequest request,
        CancellationToken cancellationToken)
    {
        // The client supplies both UTC boundaries; pass them straight through. The
        // server performs NO window derivation and is fully time-zone agnostic.
        var aggregate = await new GetWindowAggregateQuery(_readDbContext)
            .ExecuteAsync(request.StartUtc, request.EndUtc, cancellationToken);

        _logger.LogInformation(
            "Retrieved sales for window [{StartUtc:o}, {EndUtc:o}): Revenue={Revenue}, OrderCount={OrderCount}",
            request.StartUtc,
            request.EndUtc,
            aggregate.Revenue,
            aggregate.OrderCount);

        return AdminGetTodaySalesResponse.FromDomain(aggregate);
    }
}