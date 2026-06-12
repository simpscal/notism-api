using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesHandler
    : IRequestHandler<AdminGetRevenueSeriesRequest, AdminGetRevenueSeriesResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetRevenueSeriesHandler> _logger;

    public AdminGetRevenueSeriesHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetRevenueSeriesHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminGetRevenueSeriesResponse> Handle(
        AdminGetRevenueSeriesRequest request,
        CancellationToken cancellationToken)
    {
        var bucketCount = request.Boundaries.Count - 1;
        var populated = await GetRevenueByBucketsAsync(request.Boundaries, cancellationToken);
        var revenueByBucket = populated.ToDictionary(b => b.BucketIndex, b => b.Revenue);

        var points = Enumerable.Range(0, bucketCount)
            .Select(i => new RevenueSeriesPoint
            {
                Period = request.Labels[i],
                Revenue = revenueByBucket.TryGetValue(i, out var revenue) ? revenue : 0m,
            })
            .ToList();

        _logger.LogInformation(
            "Retrieved revenue series: {PopulatedCount} populated bucket(s), {PointCount} dense point(s)",
            populated.Count,
            points.Count);

        return AdminGetRevenueSeriesResponse.FromPoints(request.Granularity, points);
    }

    private async Task<IReadOnlyList<RevenueBucketTotal>> GetRevenueByBucketsAsync(
        IReadOnlyList<DateTime> boundaries,
        CancellationToken cancellationToken)
    {
        var epochBoundaries = boundaries
            .Select(b => (DateTime.SpecifyKind(b, DateTimeKind.Utc) - DateTime.UnixEpoch).TotalSeconds)
            .ToArray();

        var b0 = DateTime.SpecifyKind(boundaries[0], DateTimeKind.Utc);
        var bn = DateTime.SpecifyKind(boundaries[^1], DateTimeKind.Utc);
        var paidStatus = PaymentStatus.Paid.GetStringValue();

        FormattableString sql = $"""
            SELECT "Bucket" - 1 AS "BucketIndex",
                   SUM("TotalAmount") AS "Revenue"
            FROM (
                SELECT width_bucket(extract(epoch from "PaidAt"), {epochBoundaries}) AS "Bucket",
                       "TotalAmount"
                FROM "Orders"
                WHERE "PaymentStatus" = {paidStatus}
                  AND "PaidAt" IS NOT NULL
                  AND "PaidAt" >= {b0}
                  AND "PaidAt" < {bn}
            ) AS "Buckets"
            GROUP BY "Bucket"
            """;

        var rows = await _readDbContext.SqlQuery<RevenueBucketRow>(sql).ToListAsync(cancellationToken);

        return rows
            .Select(r => new RevenueBucketTotal(r.BucketIndex, r.Revenue))
            .OrderBy(t => t.BucketIndex)
            .ToList();
    }

    private sealed record RevenueBucketRow(int BucketIndex, decimal Revenue);
}
