using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.AdminGetOrderStatusSummary;
using Notism.Domain.Order.Repositories;

using NSubstitute;

namespace Notism.Application.Tests.Order.AdminGetOrderStatusSummary;

public class AdminGetOrderStatusSummaryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminGetOrderStatusSummaryHandler> _logger;
    private readonly AdminGetOrderStatusSummaryHandler _handler;

    public AdminGetOrderStatusSummaryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _logger = Substitute.For<ILogger<AdminGetOrderStatusSummaryHandler>>();
        _handler = new AdminGetOrderStatusSummaryHandler(_orderRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenBucketsHaveCounts_MapsCountsOntoResponse()
    {
        _orderRepository
            .GetDeliveryStatusBucketCountsAsync()
            .Returns(new OrderStatusBucketCounts(New: 3, InProgress: 5, Completed: 7));

        var result = await _handler.Handle(new AdminGetOrderStatusSummaryRequest(), CancellationToken.None);

        result.New.Should().Be(3);
        result.InProgress.Should().Be(5);
        result.Completed.Should().Be(7);
    }

    [Fact]
    public async Task Handle_WhenBucketIsEmpty_ReturnsZeroForThatBucket()
    {
        _orderRepository
            .GetDeliveryStatusBucketCountsAsync()
            .Returns(new OrderStatusBucketCounts(New: 0, InProgress: 2, Completed: 0));

        var result = await _handler.Handle(new AdminGetOrderStatusSummaryRequest(), CancellationToken.None);

        result.New.Should().Be(0);
        result.InProgress.Should().Be(2);
        result.Completed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_UsesRepositoryAggregate_AndDoesNotReaggregate()
    {
        _orderRepository
            .GetDeliveryStatusBucketCountsAsync()
            .Returns(new OrderStatusBucketCounts(New: 1, InProgress: 1, Completed: 1));

        await _handler.Handle(new AdminGetOrderStatusSummaryRequest(), CancellationToken.None);

        await _orderRepository.Received(1).GetDeliveryStatusBucketCountsAsync();
    }
}