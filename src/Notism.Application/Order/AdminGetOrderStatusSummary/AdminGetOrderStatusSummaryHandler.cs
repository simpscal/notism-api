using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminGetOrderStatusSummary;

public class AdminGetOrderStatusSummaryHandler
    : IRequestHandler<AdminGetOrderStatusSummaryRequest, AdminGetOrderStatusSummaryResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetOrderStatusSummaryHandler> _logger;

    public AdminGetOrderStatusSummaryHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetOrderStatusSummaryHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminGetOrderStatusSummaryResponse> Handle(
        AdminGetOrderStatusSummaryRequest request,
        CancellationToken cancellationToken)
    {
        // Single GROUP BY query: SQL returns at most one row per delivery status.
        var statusCounts = await _readDbContext.Set<DomainOrder>()
            .Where(o => o.DeliveryStatus != DeliveryStatus.Cancelled)
            .GroupBy(o => o.DeliveryStatus)
            .Select(group => new StatusCount(group.Key, group.Count()))
            .ToListAsync(cancellationToken);

        int CountFor(DeliveryStatus status) =>
            statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0;

        // Fold raw delivery statuses into the dashboard taxonomy. Buckets always
        // present, defaulting to zero when no orders matched that status.
        var newCount = CountFor(DeliveryStatus.OrderPlaced);
        var inProgressCount = CountFor(DeliveryStatus.Preparing) + CountFor(DeliveryStatus.OnTheWay);
        var completedCount = CountFor(DeliveryStatus.Delivered);

        var counts = new OrderStatusBucketCounts(newCount, inProgressCount, completedCount);

        _logger.LogInformation(
            "Retrieved order status summary: New={New}, InProgress={InProgress}, Completed={Completed}",
            counts.New,
            counts.InProgress,
            counts.Completed);

        return AdminGetOrderStatusSummaryResponse.FromDomain(counts);
    }

    private sealed record StatusCount(DeliveryStatus Status, int Count);
}
