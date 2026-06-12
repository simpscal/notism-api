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
        // The validator guarantees >= 2 strictly-ascending boundaries and
        // labels.Count == boundaries.Count - 1, so bucketCount >= 1.
        var bucketCount = request.Boundaries.Count - 1;

        var populated = await GetRevenueByBucketsAsync(request.Boundaries, cancellationToken);

        // Only buckets that contain Paid orders come back. Zero-fill the absent indices
        // into a dense, boundary-ordered series; this handler owns the zero-fill, never
        // the SQL.
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

    /// <summary>
    /// Raw-SQL read that buckets Paid revenue into the half-open UTC ranges
    /// <c>[boundaries[i], boundaries[i+1])</c>.
    /// <para>This is a PLAIN UTC comparison — the server derives no time-zone civil
    /// period. Bucketing uses Postgres <c>width_bucket(value, thresholds[])</c>, which is
    /// itself a half-open right-exclusive bucketer:
    ///   - returns 0       for value &lt; thresholds[1]            (before b0)
    ///   - returns i       for thresholds[i] &lt;= value &lt; thresholds[i+1]
    ///   - returns len     for value &gt;= thresholds[len]         (&gt;= bn)
    /// Feeding the n+1 epoch boundaries gives results 1..n for in-range orders; we subtract
    /// 1 to get the zero-based bucket index. The WHERE clause already restricts PaidAt to
    /// [b0, bn) so the 0 and len edge results never occur, but we keep the index
    /// translation explicit.</para>
    /// <para>The epoch doubles are used ONLY to PLACE each order in a bucket. The money is
    /// SUM("TotalAmount") in full numeric/decimal precision — epochs never enter the sum.
    /// There is NO AT TIME ZONE, NO date_trunc, NO offset/interval arithmetic; the boundary
    /// array binds as a single typed <c>double precision[]</c> parameter (no string
    /// interpolation of client values into the SQL).</para>
    /// </summary>
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
            SELECT width_bucket(extract(epoch from "PaidAt"), {epochBoundaries}) - 1 AS "BucketIndex",
                   SUM("TotalAmount") AS "Revenue"
            FROM "Orders"
            WHERE "PaymentStatus" = {paidStatus}
              AND "PaidAt" IS NOT NULL
              AND "PaidAt" >= {b0}
              AND "PaidAt" < {bn}
            GROUP BY width_bucket(extract(epoch from "PaidAt"), {epochBoundaries})
            """;

        var rows = await _readDbContext.SqlQuery<RevenueBucketRow>(sql).ToListAsync(cancellationToken);

        return rows
            .Select(r => new RevenueBucketTotal(r.BucketIndex, r.Revenue))
            .OrderBy(t => t.BucketIndex)
            .ToList();
    }

    // Row shape of the bucket aggregate. Mapped by column alias.
    private sealed record RevenueBucketRow(int BucketIndex, decimal Revenue);
}
