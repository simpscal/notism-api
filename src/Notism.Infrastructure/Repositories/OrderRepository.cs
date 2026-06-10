using Microsoft.EntityFrameworkCore;

using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Extensions;

using Npgsql;

using NpgsqlTypes;

namespace Notism.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    private readonly AppDbContext _appDbContext;

    public OrderRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<OrderStatusBucketCounts> GetDeliveryStatusBucketCountsAsync()
    {
        // Single GROUP BY query: SQL returns at most one row per delivery status.
        var statusCounts = await _dbSet
            .Where(o => o.DeliveryStatus != DeliveryStatus.Cancelled)
            .GroupBy(o => o.DeliveryStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync();

        int CountFor(DeliveryStatus status) =>
            statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0;

        // Fold raw delivery statuses into the dashboard taxonomy. Buckets always
        // present, defaulting to zero when no orders matched that status.
        var newCount = CountFor(DeliveryStatus.OrderPlaced);
        var inProgressCount = CountFor(DeliveryStatus.Preparing) + CountFor(DeliveryStatus.OnTheWay);
        var completedCount = CountFor(DeliveryStatus.Delivered);

        return new OrderStatusBucketCounts(newCount, inProgressCount, completedCount);
    }

    public async Task<OrderWindowAggregate> GetWindowAggregateAsync(DateTime startUtc, DateTime endUtc)
    {
        // Revenue: SUM(TotalAmount) over Paid orders by PaidAt window. Server-side
        // aggregate; SUM of an empty set is coalesced to 0 to keep decimal precision.
        var revenue = await _dbSet
            .Where(o => o.PaymentStatus == PaymentStatus.Paid
                && o.PaidAt != null
                && o.PaidAt >= startUtc
                && o.PaidAt < endUtc)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        // Order count: COUNT over orders by CreatedAt window. Distinct predicate.
        var orderCount = await _dbSet
            .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
            .CountAsync();

        return new OrderWindowAggregate(revenue, orderCount);
    }

    public async Task<IReadOnlyList<RevenueBucketTotal>> GetRevenueByBucketsAsync(IReadOnlyList<DateTime> boundaries)
    {
        // Bucket Paid revenue into the half-open UTC ranges [boundaries[i], boundaries[i+1]).
        //
        // This is a PLAIN UTC comparison — the server derives no time-zone civil
        // period. Bucketing uses Postgres `width_bucket(value, thresholds[])`, which
        // is itself a half-open right-exclusive bucketer:
        //   - returns 0       for value < thresholds[1]            (before b0)
        //   - returns i       for thresholds[i] <= value < thresholds[i+1]
        //   - returns len     for value >= thresholds[len]         (>= bn)
        // Feeding the n+1 epoch boundaries gives results 1..n for in-range orders; we
        // subtract 1 to get the zero-based bucket index. The WHERE clause already
        // restricts PaidAt to [b0, bn) so the 0 and len edge results never occur, but
        // we keep the index translation explicit.
        //
        // The epoch doubles are used ONLY to PLACE each order in a bucket. The money
        // is SUM("TotalAmount") in full numeric/decimal precision — epochs never enter
        // the sum. There is NO AT TIME ZONE, NO date_trunc, NO offset/interval
        // arithmetic; the boundary array binds as a single typed `double precision[]`
        // parameter (no string interpolation of client values into the SQL).
        var epochBoundaries = boundaries
            .Select(b => (DateTime.SpecifyKind(b, DateTimeKind.Utc) - DateTime.UnixEpoch).TotalSeconds)
            .ToArray();

        var b0 = DateTime.SpecifyKind(boundaries[0], DateTimeKind.Utc);
        var bn = DateTime.SpecifyKind(boundaries[^1], DateTimeKind.Utc);

        const string sql = """
            SELECT width_bucket(extract(epoch from "PaidAt"), @boundaries) - 1 AS "BucketIndex",
                   SUM("TotalAmount") AS "Revenue"
            FROM "Orders"
            WHERE "PaymentStatus" = @paidStatus
              AND "PaidAt" IS NOT NULL
              AND "PaidAt" >= @b0
              AND "PaidAt" < @bn
            GROUP BY width_bucket(extract(epoch from "PaidAt"), @boundaries)
            """;

        var boundariesParam = new NpgsqlParameter("boundaries", NpgsqlDbType.Array | NpgsqlDbType.Double)
        {
            Value = epochBoundaries,
        };
        var paidStatusParam = new NpgsqlParameter("paidStatus", PaymentStatus.Paid.GetStringValue());
        var b0Param = new NpgsqlParameter("b0", NpgsqlDbType.TimestampTz) { Value = b0 };
        var bnParam = new NpgsqlParameter("bn", NpgsqlDbType.TimestampTz) { Value = bn };

        var rows = await _appDbContext.Database
            .SqlQueryRaw<RevenueBucketRow>(sql, boundariesParam, paidStatusParam, b0Param, bnParam)
            .ToListAsync();

        return rows
            .Select(r => new RevenueBucketTotal(r.BucketIndex, r.Revenue))
            .OrderBy(t => t.BucketIndex)
            .ToList();
    }

    // Row shape of the bucket aggregate. Mapped by column alias.
    private sealed record RevenueBucketRow(int BucketIndex, decimal Revenue);
}