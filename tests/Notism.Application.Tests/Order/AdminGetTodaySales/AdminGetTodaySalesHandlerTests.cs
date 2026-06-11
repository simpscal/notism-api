using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminGetTodaySales;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.AdminGetTodaySales;

/// <summary>
/// Exercises the <see cref="GetWindowAggregateQuery"/> behind
/// <see cref="AdminGetTodaySalesHandler"/> against an EF InMemory database: revenue sums
/// Paid orders by their PaidAt window while order count counts orders by their CreatedAt
/// window (the two predicates are intentionally distinct), with empty sets coalescing to
/// zero.
/// </summary>
public class AdminGetTodaySalesHandlerTests
{
    private static readonly DateTime StartUtc = new(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EndUtc = StartUtc.AddDays(1);

    private readonly AppDbContext _dbContext;
    private readonly AdminGetTodaySalesHandler _handler;
    private Guid _userId;

    public AdminGetTodaySalesHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _handler = new AdminGetTodaySalesHandler(
            _dbContext,
            Substitute.For<ILogger<AdminGetTodaySalesHandler>>());
    }

    [Fact]
    public async Task Handle_SumsPaidRevenueWithinWindow()
    {
        await SeedUserAsync();
        var inside = StartUtc.AddHours(5);
        await SeedOrdersAsync(
            PaidOrder(100_000m, paidAt: inside, createdAt: inside),
            PaidOrder(250_000m, paidAt: inside, createdAt: inside),
            PaidOrder(999_000m, paidAt: StartUtc.AddMinutes(-1), createdAt: inside),
            PaidOrder(999_000m, paidAt: EndUtc, createdAt: inside),
            UnpaidOrder(999_000m, createdAt: inside));

        var result = await _handler.Handle(NewRequest(), CancellationToken.None);

        result.Revenue.Should().Be(350_000m);
    }

    [Fact]
    public async Task Handle_WhenNoPaidOrders_ReturnsZeroRevenue()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(UnpaidOrder(500_000m, createdAt: StartUtc.AddHours(2)));

        var result = await _handler.Handle(NewRequest(), CancellationToken.None);

        result.Revenue.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_CountsOrdersCreatedWithinWindow()
    {
        await SeedUserAsync();
        await SeedOrdersAsync(
            UnpaidOrder(0m, createdAt: StartUtc),
            UnpaidOrder(0m, createdAt: StartUtc.AddHours(12)),
            UnpaidOrder(0m, createdAt: StartUtc.AddMinutes(-1)),
            UnpaidOrder(0m, createdAt: EndUtc));

        var result = await _handler.Handle(NewRequest(), CancellationToken.None);

        result.OrderCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_RevenueAndCountUseDistinctPredicates()
    {
        await SeedUserAsync();

        // Created in window, but paid outside it: counts toward OrderCount, not Revenue.
        var order = PaidOrder(700_000m, paidAt: EndUtc.AddHours(1), createdAt: StartUtc.AddHours(1));
        await SeedOrdersAsync(order);

        var result = await _handler.Handle(NewRequest(), CancellationToken.None);

        result.OrderCount.Should().Be(1);
        result.Revenue.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_PreservesDecimalPrecision()
    {
        await SeedUserAsync();
        var inside = StartUtc.AddHours(3);
        await SeedOrdersAsync(
            PaidOrder(12_345.67m, paidAt: inside, createdAt: inside),
            PaidOrder(1.01m, paidAt: inside, createdAt: inside));

        var result = await _handler.Handle(NewRequest(), CancellationToken.None);

        result.Revenue.Should().Be(12_346.68m);
    }

    private static AdminGetTodaySalesRequest NewRequest() => new()
    {
        StartUtc = StartUtc,
        EndUtc = EndUtc,
    };

    private DomainOrder PaidOrder(decimal total, DateTime paidAt, DateTime createdAt)
        => BuildOrder(total, DeliveryStatus.OrderPlaced, PaymentStatus.Paid, paidAt, createdAt);

    private DomainOrder UnpaidOrder(decimal total, DateTime createdAt)
        => BuildOrder(total, DeliveryStatus.OrderPlaced, PaymentStatus.Unpaid, null, createdAt);

    private DomainOrder BuildOrder(decimal total, DeliveryStatus deliveryStatus, PaymentStatus paymentStatus, DateTime? paidAt, DateTime createdAt)
    {
        var order = DomainOrder.Create(_userId, PaymentMethod.Banking, new List<Guid>());
        SetProperty(order, nameof(DomainOrder.TotalAmount), total);
        SetProperty(order, nameof(DomainOrder.DeliveryStatus), deliveryStatus);
        SetProperty(order, nameof(DomainOrder.PaymentStatus), paymentStatus);
        SetProperty(order, nameof(DomainOrder.PaidAt), paidAt);
        order.CreatedAt = createdAt;
        return order;
    }

    private static void SetProperty(DomainOrder order, string name, object? value)
        => typeof(DomainOrder).GetProperty(name)!.SetValue(order, value);

    private async Task SeedUserAsync()
    {
        var user = Domain.User.User.Create("todaysales@example.com", "hashedpassword", UserRole.User);
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
