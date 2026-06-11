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
/// Verifies <see cref="OrderRepository.GetRevenueByBucketsAsync"/> against a REAL
/// PostgreSQL server. The aggregate binds an epoch boundary array as a
/// <c>double precision[]</c> parameter and buckets via <c>width_bucket(...)</c>;
/// SQLite cannot exercise that binding, and SQLite-only coverage previously masked
/// Npgsql parameter/translation failures (e.g. PostgresException 42883
/// <c>function ... does not exist</c>). This test confirms the array parameter binds
/// and executes, and that half-open boundary attribution at bucket edges is correct.
/// <para>Each run provisions an isolated throwaway database (so <c>notism_db</c> is
/// never mutated), applies EF migrations, and drops it on teardown. If the local
/// Postgres is unreachable the constructor throws with an explicit message.</para>
/// </summary>
public sealed class OrderRepositoryRevenueByBucketsPostgresTests : IAsyncLifetime
{
    // Local dev Postgres. Admin connection targets the maintenance `postgres` db so
    // we can CREATE/DROP the isolated test database.
    private const string AdminConnectionString =
        "Host=localhost;Database=postgres;Username=mac;Password=;Port=5432";

    private readonly string _databaseName = $"notism_buckettest_{Guid.NewGuid():N}";
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
    public async Task GetRevenueByBucketsAsync_BindsBoundaryArrayParameter_NoTimezoneIntervalError()
    {
        // The whole point: the double precision[] boundary parameter must bind and
        // the width_bucket query must execute without PostgresException 42883.
        var boundaries = new List<DateTime>
        {
            new(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            new(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc),
            new(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc),
        };

        await SeedOrders(
            PaidOrder(total: 100_000m, paidAt: new DateTime(2026, 6, 10, 6, 0, 0, DateTimeKind.Utc)),
            PaidOrder(total: 50_000m, paidAt: new DateTime(2026, 6, 11, 6, 0, 0, DateTimeKind.Utc)));

        var result = await _repository.GetRevenueByBucketsAsync(boundaries);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(r => r.BucketIndex == 0 && r.Revenue == 100_000m);
        result.Should().ContainSingle(r => r.BucketIndex == 1 && r.Revenue == 50_000m);
    }

    [Fact]
    public async Task GetRevenueByBucketsAsync_HalfOpenEdges_LowerBoundaryInclusive_UpperExclusive()
    {
        // Buckets: [b0, b1) = bucket 0, [b1, b2) = bucket 1.
        var b0 = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var b1 = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc);
        var b2 = new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc);
        var boundaries = new List<DateTime> { b0, b1, b2 };

        // PaidAt == b0 -> bucket 0 (lower boundary inclusive); PaidAt == b1 -> bucket 1
        // (boundary belongs to the next bucket).
        await SeedOrders(
            PaidOrder(total: 11m, paidAt: b0),
            PaidOrder(total: 22m, paidAt: b1));

        var result = await _repository.GetRevenueByBucketsAsync(boundaries);

        result.Should().ContainSingle(r => r.BucketIndex == 0 && r.Revenue == 11m);
        result.Should().ContainSingle(r => r.BucketIndex == 1 && r.Revenue == 22m);
    }

    [Fact]
    public async Task GetRevenueByBucketsAsync_ExcludesOrdersOutsideRange_IncludingUpperBoundary()
    {
        var b0 = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var b1 = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc);
        var bn = new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc);
        var boundaries = new List<DateTime> { b0, b1, bn };

        // Excluded: before b0, PaidAt == bn (upper boundary exclusive), and after bn.
        // Only the b0+5h order is in range -> bucket 0.
        await SeedOrders(
            PaidOrder(total: 1m, paidAt: b0.AddSeconds(-1)),
            PaidOrder(total: 2m, paidAt: bn),
            PaidOrder(total: 3m, paidAt: bn.AddSeconds(1)),
            PaidOrder(total: 99m, paidAt: b0.AddHours(5)));

        var result = await _repository.GetRevenueByBucketsAsync(boundaries);

        result.Should().ContainSingle();
        result[0].BucketIndex.Should().Be(0);
        result[0].Revenue.Should().Be(99m);
    }

    [Fact]
    public async Task GetRevenueByBucketsAsync_ExcludesNonPaidAndNullPaidAtOrders()
    {
        var b0 = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var bn = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc);
        var boundaries = new List<DateTime> { b0, bn };
        var inRange = b0.AddHours(5);

        await SeedOrders(
            PaidOrder(total: 200_000m, paidAt: inRange),
            UnpaidOrder(total: 999_000m, createdAt: inRange),
            FailedOrder(total: 999_000m, createdAt: inRange));

        var result = await _repository.GetRevenueByBucketsAsync(boundaries);

        result.Should().ContainSingle();
        result[0].BucketIndex.Should().Be(0);
        result[0].Revenue.Should().Be(200_000m);
    }

    [Fact]
    public async Task GetRevenueByBucketsAsync_PreservesDecimalPrecisionWithinBucket()
    {
        var b0 = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var bn = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc);
        var boundaries = new List<DateTime> { b0, bn };
        var inRange = b0.AddHours(5);

        await SeedOrders(
            PaidOrder(total: 12_345.67m, paidAt: inRange),
            PaidOrder(total: 1.01m, paidAt: inRange));

        var result = await _repository.GetRevenueByBucketsAsync(boundaries);

        result.Should().ContainSingle();
        result[0].Revenue.Should().Be(12_346.68m);
    }

    [Fact]
    public async Task GetRevenueByBucketsAsync_OmitsBucketsWithoutPaidOrders()
    {
        // Three buckets; only buckets 0 and 2 have Paid orders -> bucket 1 absent.
        var boundaries = new List<DateTime>
        {
            new(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            new(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc),
            new(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc),
            new(2026, 6, 13, 0, 0, 0, DateTimeKind.Utc),
        };

        await SeedOrders(
            PaidOrder(total: 100_000m, paidAt: new DateTime(2026, 6, 10, 6, 0, 0, DateTimeKind.Utc)),
            PaidOrder(total: 300_000m, paidAt: new DateTime(2026, 6, 12, 6, 0, 0, DateTimeKind.Utc)));

        var result = await _repository.GetRevenueByBucketsAsync(boundaries);

        result.Should().HaveCount(2);
        result.Select(r => r.BucketIndex).Should().BeEquivalentTo(new[] { 0, 2 });
        result.Should().NotContain(r => r.BucketIndex == 1);
    }

    [Fact]
    public async Task GetRevenueByBucketsAsync_WhenNoPaidOrders_ReturnsEmpty()
    {
        var boundaries = new List<DateTime>
        {
            new(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            new(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc),
        };

        await SeedOrders(UnpaidOrder(total: 500_000m, createdAt: new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc)));

        var result = await _repository.GetRevenueByBucketsAsync(boundaries);

        // Empty buckets are absent from the SQL result; the consuming handler zero-fills.
        result.Should().BeEmpty();
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
                + "GetRevenueByBucketsAsync can only be verified against a real Postgres server. "
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

    private static OrderSeed PaidOrder(decimal total, DateTime paidAt) => new()
    {
        DeliveryStatus = DeliveryStatus.OrderPlaced,
        PaymentStatus = PaymentStatus.Paid,
        TotalAmount = total,
        PaidAt = paidAt,
        CreatedAt = paidAt,
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