using System.Globalization;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminGetRevenueSeries;
using Notism.Domain.Order.Repositories;
using Notism.Shared.Utilities;

using NSubstitute;

namespace Notism.Application.Tests.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminGetRevenueSeriesHandler> _logger;
    private readonly AdminGetRevenueSeriesHandler _handler;

    public AdminGetRevenueSeriesHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<AdminGetRevenueSeriesHandler>>();
        _handler = new AdminGetRevenueSeriesHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenGranularityYear_RangesFromEarliestPaidYearThroughCurrentYear()
    {
        var currentYear = NowLocal.Year;
        var earliestYear = currentYear - 2;

        _orderRepository
            .GetRevenueByPeriodAsync(RevenuePeriodGranularity.Year)
            .Returns(new List<RevenuePeriodTotal>
            {
                Period(new DateTime(earliestYear, 1, 1), 100m),
                Period(new DateTime(currentYear, 1, 1), 300m),
            });

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest { Granularity = "year" },
            CancellationToken.None);

        result.Granularity.Should().Be("year");
        result.Points.Should().HaveCount(3);
        result.Points.Select(p => p.Period).Should().ContainInOrder(
            earliestYear.ToString(CultureInfo.InvariantCulture),
            (earliestYear + 1).ToString(CultureInfo.InvariantCulture),
            currentYear.ToString(CultureInfo.InvariantCulture));

        // Middle year is a gap and must be zero-filled.
        result.Points.First().Revenue.Should().Be(100m);
        result.Points[1].Revenue.Should().Be(0m);
        result.Points.Last().Revenue.Should().Be(300m);
    }

    [Fact]
    public async Task Handle_WhenGranularityMonth_ReturnsTwelveDenseOrderedMonthsOfCurrentYear()
    {
        var nowLocal = NowLocal;

        _orderRepository
            .GetRevenueByPeriodAsync(RevenuePeriodGranularity.Month)
            .Returns(new List<RevenuePeriodTotal>
            {
                Period(new DateTime(nowLocal.Year, 3, 1), 250m),
            });

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest { Granularity = "month" },
            CancellationToken.None);

        result.Granularity.Should().Be("month");
        result.Points.Should().HaveCount(12);

        var expectedLabels = Enumerable.Range(1, 12)
            .Select(m => $"{nowLocal.Year:D4}-{m:D2}")
            .ToList();
        result.Points.Select(p => p.Period).Should().ContainInOrder(expectedLabels);

        var march = result.Points.Single(p => p.Period == $"{nowLocal.Year:D4}-03");
        march.Revenue.Should().Be(250m);
        result.Points.Where(p => p.Period != $"{nowLocal.Year:D4}-03")
            .Should().OnlyContain(p => p.Revenue == 0m);
    }

    [Fact]
    public async Task Handle_WhenGranularityDay_ReturnsEveryDayOfCurrentMonthDenseAndOrdered()
    {
        var nowLocal = NowLocal;
        var daysInMonth = DateTime.DaysInMonth(nowLocal.Year, nowLocal.Month);

        _orderRepository
            .GetRevenueByPeriodAsync(RevenuePeriodGranularity.Day)
            .Returns(new List<RevenuePeriodTotal>
            {
                Period(new DateTime(nowLocal.Year, nowLocal.Month, 1), 42m),
            });

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest { Granularity = "day" },
            CancellationToken.None);

        result.Granularity.Should().Be("day");
        result.Points.Should().HaveCount(daysInMonth);
        result.Points.Should().BeInAscendingOrder(p => p.Period);

        var firstLabel = $"{nowLocal.Year:D4}-{nowLocal.Month:D2}-01";
        result.Points.First().Period.Should().Be(firstLabel);
        result.Points.First().Revenue.Should().Be(42m);
        result.Points.Skip(1).Should().OnlyContain(p => p.Revenue == 0m);
    }

    [Fact]
    public async Task Handle_WhenNoPaidOrdersForYear_ReturnsSingleCurrentYearZeroPoint()
    {
        _orderRepository
            .GetRevenueByPeriodAsync(RevenuePeriodGranularity.Year)
            .Returns(new List<RevenuePeriodTotal>());

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest { Granularity = "year" },
            CancellationToken.None);

        result.Points.Should().ContainSingle();
        result.Points.Single().Period.Should().Be(NowLocal.Year.ToString(CultureInfo.InvariantCulture));
        result.Points.Single().Revenue.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_WhenNoPaidOrdersForMonth_StillReturnsTwelveZeroMonths()
    {
        _orderRepository
            .GetRevenueByPeriodAsync(RevenuePeriodGranularity.Month)
            .Returns(new List<RevenuePeriodTotal>());

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest { Granularity = "month" },
            CancellationToken.None);

        result.Points.Should().HaveCount(12);
        result.Points.Should().OnlyContain(p => p.Revenue == 0m);
    }

    [Theory]
    [InlineData("year", RevenuePeriodGranularity.Year)]
    [InlineData("month", RevenuePeriodGranularity.Month)]
    [InlineData("day", RevenuePeriodGranularity.Day)]
    public async Task Handle_MapsGranularityStringOntoAggregateCall(
        string granularity,
        RevenuePeriodGranularity expected)
    {
        _orderRepository
            .GetRevenueByPeriodAsync(Arg.Any<RevenuePeriodGranularity>())
            .Returns(new List<RevenuePeriodTotal>());

        await _handler.Handle(
            new AdminGetRevenueSeriesRequest { Granularity = granularity },
            CancellationToken.None);

        await _orderRepository.Received(1).GetRevenueByPeriodAsync(expected);
    }

    private static DateTime NowLocal => DateTime.UtcNow + DayWindow.HoChiMinhOffset;

    // Build a RevenuePeriodTotal whose PeriodStartUtc is the inclusive UTC start of
    // the UTC+7 civil period beginning at the supplied local midnight.
    private static RevenuePeriodTotal Period(DateTime localMidnight, decimal revenue)
    {
        var startUtc = DateTime.SpecifyKind(
            localMidnight - DayWindow.HoChiMinhOffset,
            DateTimeKind.Utc);
        return new RevenuePeriodTotal(startUtc, revenue);
    }
}