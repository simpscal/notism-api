using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;

using DomainOrder = Notism.Domain.Order.Order;

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
        var startUtc = request.StartUtc;
        var endUtc = request.EndUtc;

        // Revenue: SUM(TotalAmount) over Paid orders by PaidAt window. Server-side
        // aggregate; SUM of an empty set is coalesced to 0 to keep decimal precision.
        var revenue = await _readDbContext.Set<DomainOrder>()
            .Where(o => o.PaymentStatus == PaymentStatus.Paid
                && o.PaidAt != null
                && o.PaidAt >= startUtc
                && o.PaidAt < endUtc)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var orderCount = await _readDbContext.Set<DomainOrder>()
            .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
            .CountAsync(cancellationToken);

        var aggregate = new OrderWindowAggregate(revenue, orderCount);

        _logger.LogInformation(
            "Retrieved sales for window [{StartUtc:o}, {EndUtc:o}): Revenue={Revenue}, OrderCount={OrderCount}",
            request.StartUtc,
            request.EndUtc,
            aggregate.Revenue,
            aggregate.OrderCount);

        return AdminGetTodaySalesResponse.FromDomain(aggregate);
    }
}