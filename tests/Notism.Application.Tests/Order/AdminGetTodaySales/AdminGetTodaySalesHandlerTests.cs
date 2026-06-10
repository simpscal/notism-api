using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminGetTodaySales;
using Notism.Domain.Order.Repositories;
using Notism.Shared.Utilities;

using NSubstitute;

namespace Notism.Application.Tests.Order.AdminGetTodaySales;

public class AdminGetTodaySalesHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminGetTodaySalesHandler> _logger;
    private readonly AdminGetTodaySalesHandler _handler;

    public AdminGetTodaySalesHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<AdminGetTodaySalesHandler>>();
        _handler = new AdminGetTodaySalesHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenOrdersExistToday_MapsAggregateOntoResponse()
    {
        _orderRepository
            .GetWindowAggregateAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new OrderWindowAggregate(Revenue: 1234.56m, OrderCount: 9));

        var result = await _handler.Handle(new AdminGetTodaySalesRequest(), CancellationToken.None);

        result.Revenue.Should().Be(1234.56m);
        result.OrderCount.Should().Be(9);
    }

    [Fact]
    public async Task Handle_WhenNoPaidOrNewOrdersToday_ReturnsZeroes()
    {
        _orderRepository
            .GetWindowAggregateAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new OrderWindowAggregate(Revenue: 0m, OrderCount: 0));

        var result = await _handler.Handle(new AdminGetTodaySalesRequest(), CancellationToken.None);

        result.Revenue.Should().Be(0m);
        result.OrderCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_QueriesRepositoryWithHoChiMinhTodayWindow()
    {
        _orderRepository
            .GetWindowAggregateAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new OrderWindowAggregate(Revenue: 0m, OrderCount: 0));

        await _handler.Handle(new AdminGetTodaySalesRequest(), CancellationToken.None);

        var (expectedStart, expectedEnd) = DayWindow.HoChiMinhDay(DateTime.UtcNow);

        await _orderRepository.Received(1).GetWindowAggregateAsync(
            Arg.Is<DateTime>(start => start == expectedStart),
            Arg.Is<DateTime>(end => end == expectedEnd));
    }

    [Fact]
    public async Task Handle_DoesNotReaggregate_UsesRepositoryAggregateDirectly()
    {
        _orderRepository
            .GetWindowAggregateAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(new OrderWindowAggregate(Revenue: 42m, OrderCount: 1));

        await _handler.Handle(new AdminGetTodaySalesRequest(), CancellationToken.None);

        await _orderRepository.Received(1).GetWindowAggregateAsync(
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>());
    }
}