using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminGetRevenueSeries;
using Notism.Application.Tests.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;

using NSubstitute;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Tests.Order.AdminGetRevenueSeries;

public sealed class AdminGetRevenueSeriesHandlerTests : IClassFixture<PostgresReadDbContextFixture>, IAsyncLifetime
{
    private static readonly DateTime SeriesStart = new(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly AppDbContext _dbContext;
    private readonly AdminGetRevenueSeriesHandler _handler;

    public AdminGetRevenueSeriesHandlerTests(PostgresReadDbContextFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _handler = new AdminGetRevenueSeriesHandler(
            _dbContext,
            Substitute.For<ILogger<AdminGetRevenueSeriesHandler>>());
    }

    // The container is shared across the class; clear seeded orders before each test so
    // revenue from one test never leaks into another.
    public async Task InitializeAsync()
    {
        await _dbContext.Orders.ExecuteDeleteAsync();
        _dbContext.ChangeTracker.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ReturnsDenseOrderedSeries_OneLabelledPointPerBucket()
    {
        var userId = await SeedUserAsync();
        await SeedPaidOrderAsync(userId, 100m, SeriesStart.AddHours(6));
        await SeedPaidOrderAsync(userId, 300m, SeriesStart.AddDays(1).AddHours(6));

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = Boundaries(3),
                Labels = new List<string> { "a", "b" },
                Granularity = "day",
            },
            CancellationToken.None);

        result.Granularity.Should().Be("day");
        result.Points.Should().HaveCount(2);
        result.Points.Select(p => p.Period).Should().ContainInOrder("a", "b");
        result.Points[0].Revenue.Should().Be(100m);
        result.Points[1].Revenue.Should().Be(300m);
    }

    [Fact]
    public async Task Handle_ZeroFillsBucketsAbsentFromSqlResult()
    {
        var userId = await SeedUserAsync();

        // Only the middle bucket (index 1) has a Paid order.
        await SeedPaidOrderAsync(userId, 250m, SeriesStart.AddDays(1).AddHours(6));

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = Boundaries(4),
                Labels = new List<string> { "x", "y", "z" },
            },
            CancellationToken.None);

        result.Points.Should().HaveCount(3);
        result.Points[0].Revenue.Should().Be(0m);
        result.Points[1].Revenue.Should().Be(250m);
        result.Points[2].Revenue.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WhenNoPaidOrders_ReturnsDenseZeroSeries()
    {
        await SeedUserAsync();

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = Boundaries(4),
                Labels = new List<string> { "x", "y", "z" },
            },
            CancellationToken.None);

        result.Points.Should().HaveCount(3);
        result.Points.Should().OnlyContain(p => p.Revenue == 0m);
    }

    [Fact]
    public async Task Handle_SingleBucket_ReturnsSinglePoint()
    {
        var userId = await SeedUserAsync();
        await SeedPaidOrderAsync(userId, 42m, SeriesStart.AddHours(6));

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = Boundaries(2),
                Labels = new List<string> { "only" },
            },
            CancellationToken.None);

        result.Points.Should().ContainSingle();
        result.Points[0].Period.Should().Be("only");
        result.Points[0].Revenue.Should().Be(42m);
    }

    [Fact]
    public async Task Handle_EchoesGranularityHintVerbatim()
    {
        await SeedUserAsync();

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = Boundaries(2),
                Labels = new List<string> { "only" },
                Granularity = "month",
            },
            CancellationToken.None);

        result.Granularity.Should().Be("month");
    }

    private static List<DateTime> Boundaries(int count)
        => Enumerable.Range(0, count).Select(i => SeriesStart.AddDays(i)).ToList();

    private async Task<Guid> SeedUserAsync()
    {
        var user = Domain.User.User.Create($"revenue-{Guid.NewGuid():N}@example.com", "hashedpassword", UserRole.User);
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return user.Id;
    }

    private async Task SeedPaidOrderAsync(Guid userId, decimal total, DateTime paidAt)
    {
        var order = DomainOrder.Create(userId, PaymentMethod.Banking, new List<Guid>());
        order.ClearDomainEvents();
        typeof(DomainOrder).GetProperty(nameof(DomainOrder.TotalAmount))!.SetValue(order, total);
        typeof(DomainOrder).GetProperty(nameof(DomainOrder.PaymentStatus))!.SetValue(order, PaymentStatus.Paid);
        typeof(DomainOrder).GetProperty(nameof(DomainOrder.PaidAt))!.SetValue(order, paidAt);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}