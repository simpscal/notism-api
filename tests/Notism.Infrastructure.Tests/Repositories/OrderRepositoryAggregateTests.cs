using FluentAssertions;

using MediatR;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Infrastructure.Repositories;

using NSubstitute;

namespace Notism.Infrastructure.Tests.Repositories;

public sealed class OrderRepositoryAggregateTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _dbContext;
    private readonly OrderRepository _repository;
    private readonly Guid _userId;
    private readonly List<string> _sqlLog = new();

    public OrderRepositoryAggregateTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .LogTo(_sqlLog.Add, Microsoft.Extensions.Logging.LogLevel.Information)
            .Options;

        _dbContext = new SqliteTestDbContext(options, Substitute.For<IMediator>());
        _dbContext.Database.EnsureCreated();

        var user = Domain.User.User.Create("dashboard@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
        _userId = user.Id;
        _dbContext.ChangeTracker.Clear();

        _repository = new OrderRepository(_dbContext);
    }

    [Fact]
    public async Task GetDeliveryStatusBucketCountsAsync_FoldsStatusesIntoBuckets()
    {
        await SeedOrders(
            OrderWith(DeliveryStatus.OrderPlaced),
            OrderWith(DeliveryStatus.OrderPlaced),
            OrderWith(DeliveryStatus.Preparing),
            OrderWith(DeliveryStatus.OnTheWay),
            OrderWith(DeliveryStatus.OnTheWay),
            OrderWith(DeliveryStatus.Delivered));

        var result = await _repository.GetDeliveryStatusBucketCountsAsync();

        result.New.Should().Be(2);
        result.InProgress.Should().Be(3);
        result.Completed.Should().Be(1);
    }

    [Fact]
    public async Task GetDeliveryStatusBucketCountsAsync_ExcludesCancelledOrders()
    {
        await SeedOrders(
            OrderWith(DeliveryStatus.OrderPlaced),
            OrderWith(DeliveryStatus.Cancelled),
            OrderWith(DeliveryStatus.Cancelled));

        var result = await _repository.GetDeliveryStatusBucketCountsAsync();

        result.New.Should().Be(1);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(0);
    }

    [Fact]
    public async Task GetDeliveryStatusBucketCountsAsync_WhenNoOrders_ReturnsZeroBuckets()
    {
        var result = await _repository.GetDeliveryStatusBucketCountsAsync();

        result.New.Should().Be(0);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(0);
    }

    [Fact]
    public async Task GetDeliveryStatusBucketCountsAsync_WhenBucketEmpty_ReturnsZeroForThatBucket()
    {
        await SeedOrders(OrderWith(DeliveryStatus.Delivered));

        var result = await _repository.GetDeliveryStatusBucketCountsAsync();

        result.New.Should().Be(0);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(1);
    }

    [Fact]
    public async Task GetWindowAggregateAsync_SumsPaidRevenueWithinWindow()
    {
        var start = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);
        var inside = start.AddHours(5);

        var paidInside1 = PaidOrder(total: 100_000m, paidAt: inside, createdAt: inside);
        var paidInside2 = PaidOrder(total: 250_000m, paidAt: inside, createdAt: inside);
        var paidBeforeWindow = PaidOrder(total: 999_000m, paidAt: start.AddMinutes(-1), createdAt: inside);
        var paidAtExclusiveEnd = PaidOrder(total: 999_000m, paidAt: end, createdAt: inside);
        var unpaidInside = UnpaidOrder(total: 999_000m, createdAt: inside);

        await SeedOrders(paidInside1, paidInside2, paidBeforeWindow, paidAtExclusiveEnd, unpaidInside);

        var result = await _repository.GetWindowAggregateAsync(start, end);

        result.Revenue.Should().Be(350_000m);
    }

    [Fact]
    public async Task GetWindowAggregateAsync_WhenNoPaidOrders_ReturnsZeroRevenue()
    {
        var start = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        await SeedOrders(UnpaidOrder(total: 500_000m, createdAt: start.AddHours(2)));

        var result = await _repository.GetWindowAggregateAsync(start, end);

        result.Revenue.Should().Be(0m);
    }

    [Fact]
    public async Task GetWindowAggregateAsync_CountsOrdersCreatedWithinWindow()
    {
        var start = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var createdAtStart = UnpaidOrder(total: 0m, createdAt: start);
        var createdMidWindow = UnpaidOrder(total: 0m, createdAt: start.AddHours(12));
        var createdBeforeWindow = UnpaidOrder(total: 0m, createdAt: start.AddMinutes(-1));
        var createdAtExclusiveEnd = UnpaidOrder(total: 0m, createdAt: end);

        await SeedOrders(createdAtStart, createdMidWindow, createdBeforeWindow, createdAtExclusiveEnd);

        var result = await _repository.GetWindowAggregateAsync(start, end);

        result.OrderCount.Should().Be(2);
    }

    [Fact]
    public async Task GetWindowAggregateAsync_RevenueAndCountUseDistinctPredicates()
    {
        var start = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);

        // Created in window, but paid outside it: counts toward OrderCount, not Revenue.
        await SeedOrders(
            new OrderSeed
            {
                DeliveryStatus = DeliveryStatus.OrderPlaced,
                PaymentStatus = PaymentStatus.Paid,
                TotalAmount = 700_000m,
                CreatedAt = start.AddHours(1),
                PaidAt = end.AddHours(1),
            });

        var result = await _repository.GetWindowAggregateAsync(start, end);

        result.OrderCount.Should().Be(1);
        result.Revenue.Should().Be(0m);
    }

    [Fact]
    public async Task GetWindowAggregateAsync_PreservesDecimalPrecision()
    {
        var start = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);
        var inside = start.AddHours(3);

        await SeedOrders(
            PaidOrder(total: 12_345.67m, paidAt: inside, createdAt: inside),
            PaidOrder(total: 1.01m, paidAt: inside, createdAt: inside));

        var result = await _repository.GetWindowAggregateAsync(start, end);

        result.Revenue.Should().Be(12_346.68m);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_Day_BucketsByHoChiMinhCivilDay()
    {
        // 2026-06-10T17:30:00Z is 2026-06-11T00:30:00 in UTC+7 -> the 2026-06-11 day bucket.
        // 2026-06-10T16:30:00Z is 2026-06-10T23:30:00 in UTC+7 -> the 2026-06-10 day bucket.
        var lateUtc = new DateTime(2026, 6, 10, 17, 30, 0, DateTimeKind.Utc);
        var earlyUtc = new DateTime(2026, 6, 10, 16, 30, 0, DateTimeKind.Utc);

        await SeedOrders(
            PaidOrder(total: 100_000m, paidAt: lateUtc, createdAt: lateUtc),
            PaidOrder(total: 50_000m, paidAt: earlyUtc, createdAt: earlyUtc));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day);

        // Day buckets start at local midnight = 17:00Z the prior UTC day.
        var june10StartUtc = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
        var june11StartUtc = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.PeriodStartUtc == june10StartUtc && r.Revenue == 50_000m);
        result.Should().ContainSingle(r => r.PeriodStartUtc == june11StartUtc && r.Revenue == 100_000m);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_Day_AttributesUtcBoundaryToNextCivilDay()
    {
        // Exactly on the UTC->UTC+7 day boundary: 17:00:00Z == local midnight of the next civil day.
        var boundaryUtc = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc);

        await SeedOrders(PaidOrder(total: 7_777m, paidAt: boundaryUtc, createdAt: boundaryUtc));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day);

        var june11StartUtc = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc);

        result.Should().ContainSingle();
        result[0].PeriodStartUtc.Should().Be(june11StartUtc);
        result[0].Revenue.Should().Be(7_777m);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_Month_BucketsByHoChiMinhCivilMonth()
    {
        // 2026-06-30T17:30:00Z is 2026-07-01T00:30:00 in UTC+7 -> the July bucket.
        var julyLocalUtc = new DateTime(2026, 6, 30, 17, 30, 0, DateTimeKind.Utc);
        var juneLocalUtc = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        await SeedOrders(
            PaidOrder(total: 300_000m, paidAt: julyLocalUtc, createdAt: julyLocalUtc),
            PaidOrder(total: 120_000m, paidAt: juneLocalUtc, createdAt: juneLocalUtc));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Month);

        // Month buckets start at local first-of-month midnight = 17:00Z the prior UTC day.
        var juneStartUtc = new DateTime(2026, 5, 31, 17, 0, 0, DateTimeKind.Utc);
        var julyStartUtc = new DateTime(2026, 6, 30, 17, 0, 0, DateTimeKind.Utc);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.PeriodStartUtc == juneStartUtc && r.Revenue == 120_000m);
        result.Should().ContainSingle(r => r.PeriodStartUtc == julyStartUtc && r.Revenue == 300_000m);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_Year_BucketsByHoChiMinhCivilYear()
    {
        // 2025-12-31T17:30:00Z is 2026-01-01T00:30:00 in UTC+7 -> the 2026 bucket.
        var year2026LocalUtc = new DateTime(2025, 12, 31, 17, 30, 0, DateTimeKind.Utc);
        var year2025LocalUtc = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        await SeedOrders(
            PaidOrder(total: 1_000_000m, paidAt: year2026LocalUtc, createdAt: year2026LocalUtc),
            PaidOrder(total: 400_000m, paidAt: year2025LocalUtc, createdAt: year2025LocalUtc));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Year);

        // Year buckets start at local Jan-1 midnight = 17:00Z the prior UTC day.
        var year2025StartUtc = new DateTime(2024, 12, 31, 17, 0, 0, DateTimeKind.Utc);
        var year2026StartUtc = new DateTime(2025, 12, 31, 17, 0, 0, DateTimeKind.Utc);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.PeriodStartUtc == year2025StartUtc && r.Revenue == 400_000m);
        result.Should().ContainSingle(r => r.PeriodStartUtc == year2026StartUtc && r.Revenue == 1_000_000m);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_ExcludesNonPaidOrders()
    {
        var paidAt = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc);

        await SeedOrders(
            PaidOrder(total: 200_000m, paidAt: paidAt, createdAt: paidAt),
            UnpaidOrder(total: 999_000m, createdAt: paidAt),
            FailedOrder(total: 999_000m, createdAt: paidAt));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day);

        result.Should().ContainSingle();
        result[0].Revenue.Should().Be(200_000m);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_PreservesDecimalPrecisionWithinBucket()
    {
        var paidAt = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc);

        await SeedOrders(
            PaidOrder(total: 12_345.67m, paidAt: paidAt, createdAt: paidAt),
            PaidOrder(total: 1.01m, paidAt: paidAt, createdAt: paidAt));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day);

        result.Should().ContainSingle();
        result[0].Revenue.Should().Be(12_346.68m);
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_WhenNoPaidOrders_ReturnsEmpty()
    {
        await SeedOrders(UnpaidOrder(total: 500_000m, createdAt: new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc)));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day);

        // Empty periods are absent from the SQL result; the consuming handler zero-fills.
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_OmitsPeriodsWithoutPaidOrders()
    {
        // Two distinct civil days have Paid orders; the gap day in between is absent.
        var day1 = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc);
        var day3 = new DateTime(2026, 6, 12, 10, 0, 0, DateTimeKind.Utc);

        await SeedOrders(
            PaidOrder(total: 100_000m, paidAt: day1, createdAt: day1),
            PaidOrder(total: 300_000m, paidAt: day3, createdAt: day3));

        var result = await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day);

        var day1StartUtc = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
        var day3StartUtc = new DateTime(2026, 6, 11, 17, 0, 0, DateTimeKind.Utc);

        result.Should().HaveCount(2);
        result.Select(r => r.PeriodStartUtc).Should().BeEquivalentTo(new[] { day1StartUtc, day3StartUtc });

        // The 2026-06-11 (UTC+7) day bucket has no Paid orders and is absent.
        result.Should().NotContain(r => r.PeriodStartUtc == new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_RunsAsSingleServerSideGroupByQuery()
    {
        var paidAt = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc);
        await SeedOrders(PaidOrder(total: 100_000m, paidAt: paidAt, createdAt: paidAt));

        _sqlLog.Clear();

        await _repository.GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day);

        // The aggregate must execute as a single server-side GROUP BY: exactly one
        // SELECT, containing GROUP BY and SUM, with no client-side row materialization.
        var selects = _sqlLog
            .Where(l => l.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        selects.Should().ContainSingle();
        var sql = selects[0];
        sql.Should().Contain("GROUP BY");
        sql.Should().Contain("SUM(", "the revenue total must be summed server-side");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private async Task SeedOrders(params OrderSeed[] seeds)
    {
        foreach (var seed in seeds)
        {
            _dbContext.Orders.Add(seed.Build(_userId));
        }

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }

    private static OrderSeed OrderWith(DeliveryStatus status) => new()
    {
        DeliveryStatus = status,
        PaymentStatus = PaymentStatus.Unpaid,
        TotalAmount = 0m,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    private static OrderSeed PaidOrder(decimal total, DateTime paidAt, DateTime createdAt) => new()
    {
        DeliveryStatus = DeliveryStatus.OrderPlaced,
        PaymentStatus = PaymentStatus.Paid,
        TotalAmount = total,
        PaidAt = paidAt,
        CreatedAt = createdAt,
    };

    private static OrderSeed UnpaidOrder(decimal total, DateTime createdAt) => new()
    {
        DeliveryStatus = DeliveryStatus.OrderPlaced,
        PaymentStatus = PaymentStatus.Unpaid,
        TotalAmount = total,
        CreatedAt = createdAt,
    };

    private static OrderSeed FailedOrder(decimal total, DateTime createdAt) => new()
    {
        DeliveryStatus = DeliveryStatus.OrderPlaced,
        PaymentStatus = PaymentStatus.Failed,
        TotalAmount = total,
        CreatedAt = createdAt,
    };

    private sealed class OrderSeed
    {
        public DeliveryStatus DeliveryStatus { get; init; }
        public PaymentStatus PaymentStatus { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime? PaidAt { get; init; }
        public DateTime CreatedAt { get; init; }

        public Order Build(Guid userId)
        {
            var order = Order.Create(userId, PaymentMethod.Banking, new List<Guid>());
            order.ClearDomainEvents();

            SetProperty(order, nameof(Order.TotalAmount), TotalAmount);
            SetProperty(order, nameof(Order.DeliveryStatus), DeliveryStatus);
            SetProperty(order, nameof(Order.PaymentStatus), PaymentStatus);
            SetProperty(order, nameof(Order.PaidAt), PaidAt);
            order.CreatedAt = CreatedAt;

            return order;
        }

        private static void SetProperty(Order order, string propertyName, object? value)
        {
            typeof(Order)
                .GetProperty(propertyName)!
                .SetValue(order, value);
        }
    }
}