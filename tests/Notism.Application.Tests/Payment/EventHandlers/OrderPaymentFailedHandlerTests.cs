using FluentAssertions;

using Notism.Application.Common.Interfaces;
using Notism.Application.Payment.EventHandlers;
using Notism.Domain.Order.Events;

using NSubstitute;

namespace Notism.Application.Tests.Payment.EventHandlers;

public class OrderPaymentFailedHandlerTests
{
    private readonly INotificationService _notificationService;
    private readonly OrderPaymentFailedHandler _handler;

    public OrderPaymentFailedHandlerTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        _handler = new OrderPaymentFailedHandler(_notificationService);
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentFailedEventReceived_SendsPaymentFailureToUsersGroup()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var notification = new OrderPaymentFailedEvent(orderId, userId);

        await _handler.Handle(notification, CancellationToken.None);

        await _notificationService.Received(1).NotifyPaymentFailureAsync(
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

        await _notificationService.Received(1).NotifyPaymentFailureAsync(
            orderId, userId, Arg.Any<CancellationToken>());
        await _notificationService.DidNotReceive().NotifyPaymentFailureAsync(
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
