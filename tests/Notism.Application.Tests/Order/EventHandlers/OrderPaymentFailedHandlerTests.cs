using FluentAssertions;

using Notism.Application.Common.Services;
using Notism.Application.Order.EventHandlers;
using Notism.Domain.Order.Events;

using NSubstitute;

namespace Notism.Application.Tests.Order.EventHandlers;

public class OrderPaymentFailedHandlerTests
{
    private readonly IPaymentNotifier _paymentNotifier;
    private readonly OrderPaymentFailedHandler _handler;

    public OrderPaymentFailedHandlerTests()
    {
        _paymentNotifier = Substitute.For<IPaymentNotifier>();
        _handler = new OrderPaymentFailedHandler(_paymentNotifier);
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentFailedEventReceived_SendsPaymentFailureToUsersGroup()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var notification = new OrderPaymentFailedEvent(orderId, userId);

        await _handler.Handle(notification, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyPaymentFailureAsync(
            orderId, userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentFailedEventReceived_SendsToCorrectUserGroup()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var notification = new OrderPaymentFailedEvent(orderId, userId);

        await _handler.Handle(notification, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyPaymentFailureAsync(
            orderId, userId, Arg.Any<CancellationToken>());
        await _paymentNotifier.DidNotReceive().NotifyPaymentFailureAsync(
            Arg.Any<Guid>(), otherUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentFailedEventReceived_DoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var notification = new OrderPaymentFailedEvent(orderId, userId);

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}