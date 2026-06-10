using FluentAssertions;

using MediatR;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
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

    public OrderRepositoryAggregateTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
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