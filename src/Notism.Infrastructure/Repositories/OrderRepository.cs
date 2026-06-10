using Microsoft.EntityFrameworkCore;

using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Extensions;
using Notism.Shared.Utilities;

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

    public async Task<IReadOnlyList<RevenuePeriodTotal>> GetRevenueByPeriodAsync(RevenuePeriodGranularity granularity)
    {
        // Bucket Paid revenue by the Asia/Ho_Chi_Minh (UTC+7) civil period of PaidAt.
        //
        // PaidAt is a `timestamp with time zone` (timestamptz). The correct Postgres
        // idiom for civil-period bucketing is to convert it to local civil time with
        // `AT TIME ZONE 'Asia/Ho_Chi_Minh'` (which yields a `timestamp without time
        // zone`) and then `date_trunc(...)` to the period. We must NOT shift the
        // timestamptz by an interval and extract date-parts: that forces Npgsql to
        // emit `timezone(unknown, interval)`, which does not exist (PostgresException
        // 42883) and crashes at runtime against real Postgres.
        //
        // Expressed as raw SQL on the DbSet so the whole aggregate runs as a single
        // server-side GROUP BY query: filtered to Paid + PaidAt NOT NULL, grouped by
        // the truncated local-civil period, SUM(TotalAmount) per bucket. SUM keeps
        // full decimal precision (numeric). Only populated buckets are returned.
        var truncField = granularity switch
        {
            RevenuePeriodGranularity.Year => "year",
            RevenuePeriodGranularity.Month => "month",
            _ => "day",
        };

        // date_trunc yields the local-civil period start as a `timestamp without time
        // zone`. We read it as the unspecified civil instant and convert it back to
        // the UTC instant below (subtract the +7h offset), consistent with DayWindow.
        var sql = $$"""
            SELECT date_trunc('{{truncField}}', "PaidAt" AT TIME ZONE 'Asia/Ho_Chi_Minh') AS "PeriodStartLocal",
                   SUM("TotalAmount") AS "Revenue"
            FROM "Orders"
            WHERE "PaymentStatus" = {0} AND "PaidAt" IS NOT NULL
            GROUP BY date_trunc('{{truncField}}', "PaidAt" AT TIME ZONE 'Asia/Ho_Chi_Minh')
            """;

        var rows = await _appDbContext.Database
            .SqlQueryRaw<RevenuePeriodRow>(sql, PaymentStatus.Paid.GetStringValue())
            .ToListAsync();

        return rows
            .Select(r => new RevenuePeriodTotal(LocalPeriodStartUtc(r.PeriodStartLocal), r.Revenue))
            .OrderBy(t => t.PeriodStartUtc)
            .ToList();
    }

    // Row shape of the period-bucket aggregate. PeriodStartLocal is the truncated
    // local-civil period start (timestamp without time zone). Mapped by column alias.
    private sealed record RevenuePeriodRow(DateTime PeriodStartLocal, decimal Revenue);

    // Reconstruct the inclusive UTC start of a UTC+7 civil period from its local
    // civil-period start: treat it as local civil time, then shift back by the offset.
    private static DateTime LocalPeriodStartUtc(DateTime localPeriodStart)
    {
        var localMidnight = DateTime.SpecifyKind(localPeriodStart, DateTimeKind.Unspecified);
        return DateTime.SpecifyKind(localMidnight - DayWindow.HoChiMinhOffset, DateTimeKind.Utc);
    }
}