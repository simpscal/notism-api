using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminGetOrderStatusSummary;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.AdminGetOrderStatusSummary;

/// <summary>
/// Exercises the <see cref="GetDeliveryStatusBucketCountsQuery"/> behind
/// <see cref="AdminGetOrderStatusSummaryHandler"/> against an EF InMemory database: the
/// fold of raw delivery statuses into the New / InProgress / Completed dashboard buckets,
/// with cancelled orders excluded and empty buckets defaulting to zero.
/// </summary>
public class AdminGetOrderStatusSummaryHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly AdminGetOrderStatusSummaryHandler _handler;
    private Guid _userId;

    public AdminGetOrderStatusSummaryHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _handler = new AdminGetOrderStatusSummaryHandler(
            _dbContext,
            Substitute.For<ILogger<AdminGetOrderStatusSummaryHandler>>());
    }

    [Fact]
    public async Task Handle_FoldsStatusesIntoBuckets()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(
            OrderWith(DeliveryStatus.OrderPlaced),
            OrderWith(DeliveryStatus.OrderPlaced),
            OrderWith(DeliveryStatus.Preparing),
            OrderWith(DeliveryStatus.OnTheWay),
            OrderWith(DeliveryStatus.OnTheWay),
            OrderWith(DeliveryStatus.Delivered));

        var result = await _handler.Handle(new AdminGetOrderStatusSummaryRequest(), CancellationToken.None);

        result.New.Should().Be(2);
        result.InProgress.Should().Be(3);
        result.Completed.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ExcludesCancelledOrders()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(
            OrderWith(DeliveryStatus.OrderPlaced),
            OrderWith(DeliveryStatus.Cancelled),
            OrderWith(DeliveryStatus.Cancelled));

        var result = await _handler.Handle(new AdminGetOrderStatusSummaryRequest(), CancellationToken.None);

        result.New.Should().Be(1);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenNoOrders_ReturnsZeroBuckets()
    {
        await SeedUserAsync();

        var result = await _handler.Handle(new AdminGetOrderStatusSummaryRequest(), CancellationToken.None);

        result.New.Should().Be(0);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenBucketEmpty_ReturnsZeroForThatBucket()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(OrderWith(DeliveryStatus.Delivered));

        var result = await _handler.Handle(new AdminGetOrderStatusSummaryRequest(), CancellationToken.None);

        result.New.Should().Be(0);
        result.InProgress.Should().Be(0);
        result.Completed.Should().Be(1);
    }

    private DomainOrder OrderWith(DeliveryStatus status)
    {
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        typeof(DomainOrder).GetProperty(nameof(DomainOrder.DeliveryStatus))!.SetValue(order, status);
        return order;
    }

    private async Task SeedUserAsync()
    {
        var user = Domain.User.User.Create("summary@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _userId = user.Id;
        _dbContext.ChangeTracker.Clear();
    }

    private async Task SeedOrdersAsync(params DomainOrder[] orders)
    {
        foreach (var order in orders)
        {
            order.ClearDomainEvents();
            _dbContext.Orders.Add(order);
        }

        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}
