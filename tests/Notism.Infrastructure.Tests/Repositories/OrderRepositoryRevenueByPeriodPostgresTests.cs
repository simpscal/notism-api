using FluentAssertions;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Infrastructure.Repositories;

using Npgsql;

using NSubstitute;

namespace Notism.Infrastructure.Tests.Repositories;

/// <summary>
/// Verifies <see cref="OrderRepository.GetRevenueByPeriodAsync"/> against a REAL
/// PostgreSQL server. The aggregate uses the Postgres-only
/// <c>AT TIME ZONE 'Asia/Ho_Chi_Minh'</c> + <c>date_trunc</c> idiom, which SQLite
/// cannot translate; SQLite-only coverage previously masked PostgresException 42883
/// (<c>function pg_catalog.timezone(unknown, interval) does not exist</c>).
/// <para>Each run provisions an isolated throwaway database (so <c>notism_db</c> is
/// never mutated), applies EF migrations, and drops it on teardown. If the local
/// Postgres is unreachable the constructor throws with an explicit message.</para>
/// </summary>
public sealed class OrderRepositoryRevenueByPeriodPostgresTests : IAsyncLifetime
{
    // Local dev Postgres. Admin connection targets the maintenance `postgres` db so
    // we can CREATE/DROP the isolated test database.
    private const string AdminConnectionString =
        "Host=localhost;Database=postgres;Username=mac;Password=;Port=5432";

    private readonly string _databaseName = $"notism_revtest_{Guid.NewGuid():N}";
    private AppDbContext _dbContext = null!;
    private OrderRepository _repository = null!;
    private Guid _userId;

    private string TestConnectionString =>
        $"Host=localhost;Database={_databaseName};Username=mac;Password=;Port=5432";

    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(TestConnectionString)
            .Options;

        _dbContext = new AppDbContext(options, Substitute.For<IMediator>());
        await _dbContext.Database.MigrateAsync();

        var user = Domain.User.User.Create("dashboard@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _userId = user.Id;
        _dbContext.ChangeTracker.Clear();

        _repository = new OrderRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.DisposeAsync();
        }

        await DropDatabaseAsync();
    }

    [Fact]
    public async Task GetRevenueByPeriodAsync_Day_BucketsByHoChiMinhCivilDay_NoTimezoneIntervalError()
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

    private async Task SeedOrders(params OrderSeed[] seeds)
    {
        foreach (var seed in seeds)
        {
            _dbContext.Orders.Add(seed.Build(_userId));
        }

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }

    private async Task CreateDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnectionString);

        try
        {
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Local PostgreSQL is unreachable at Host=localhost;Port=5432 (Username=mac). "
                + "GetRevenueByPeriodAsync can only be verified against a real Postgres server. "
                + $"Underlying error: {ex.Message}",
                ex);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
        await command.ExecuteNonQueryAsync();
    }

    private async Task DropDatabaseAsync()
    {
        NpgsqlConnection.ClearAllPools();

        await using var connection = new NpgsqlConnection(AdminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)";
        await command.ExecuteNonQueryAsync();
    }

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