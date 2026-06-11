using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminGetRevenueSeries;
using Notism.Domain.Order.Repositories;

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
    public async Task Handle_ReturnsDenseOrderedSeries_OneLabelledPointPerBucket()
    {
        var boundaries = Boundaries(3);

        _orderRepository
            .GetRevenueByBucketsAsync(Arg.Any<IReadOnlyList<DateTime>>())
            .Returns(new List<RevenueBucketTotal>
            {
                new(0, 100m),
                new(1, 300m),
            });

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = boundaries,
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
    public async Task Handle_ZeroFillsBucketsAbsentFromRepositoryResult()
    {
        var boundaries = Boundaries(4);

        // Only the middle bucket has Paid orders; buckets 0 and 2 are absent.
        _orderRepository
            .GetRevenueByBucketsAsync(Arg.Any<IReadOnlyList<DateTime>>())
            .Returns(new List<RevenueBucketTotal> { new(1, 250m) });

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = boundaries,
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
        var boundaries = Boundaries(4);

        _orderRepository
            .GetRevenueByBucketsAsync(Arg.Any<IReadOnlyList<DateTime>>())
            .Returns(new List<RevenueBucketTotal>());

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = boundaries,
                Labels = new List<string> { "x", "y", "z" },
            },
            CancellationToken.None);

        result.Points.Should().HaveCount(3);
        result.Points.Should().OnlyContain(p => p.Revenue == 0m);
    }

    [Fact]
    public async Task Handle_SingleBucket_ReturnsSinglePoint()
    {
        var boundaries = Boundaries(2);

        _orderRepository
            .GetRevenueByBucketsAsync(Arg.Any<IReadOnlyList<DateTime>>())
            .Returns(new List<RevenueBucketTotal> { new(0, 42m) });

        var result = await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = boundaries,
                Labels = new List<string> { "only" },
            },
            CancellationToken.None);

        result.Points.Should().ContainSingle();
        result.Points[0].Period.Should().Be("only");
        result.Points[0].Revenue.Should().Be(42m);
    }

    [Fact]
    public async Task Handle_PassesClientBoundariesStraightToRepository()
    {
        var boundaries = Boundaries(3);

        _orderRepository
            .GetRevenueByBucketsAsync(Arg.Any<IReadOnlyList<DateTime>>())
            .Returns(new List<RevenueBucketTotal>());

        await _handler.Handle(
            new AdminGetRevenueSeriesRequest
            {
                Boundaries = boundaries,
                Labels = new List<string> { "a", "b" },
            },
            CancellationToken.None);

        await _orderRepository.Received(1).GetRevenueByBucketsAsync(
            Arg.Is<IReadOnlyList<DateTime>>(b => b.SequenceEqual(boundaries)));
    }

    [Fact]
    public async Task Handle_EchoesGranularityHintVerbatim()
    {
        _orderRepository
            .GetRevenueByBucketsAsync(Arg.Any<IReadOnlyList<DateTime>>())
            .Returns(new List<RevenueBucketTotal>());

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
    {
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        return Enumerable.Range(0, count)
            .Select(i => start.AddDays(i))
            .ToList();
    }
}