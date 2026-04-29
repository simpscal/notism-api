using FluentAssertions;

using Notism.Application.Common.Interfaces;
using Notism.Application.Payment.EventHandlers;
using Notism.Domain.Order.Events;

using NSubstitute;

namespace Notism.Application.Tests.Payment.EventHandlers;

public class OrderPaidHandlerTests
{
    private readonly INotificationService _notificationService;
    private readonly OrderPaidHandler _handler;

    public OrderPaidHandlerTests()
    {
        _notificationService = Substitute.For<INotificationService>();
        _handler = new OrderPaidHandler(_notificationService);
    }

    [Fact]
    public async Task Handle_WhenOrderPaidEventReceived_SendsPaymentSuccessToUsersGroup()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var paidAt = new DateTime(2026, 4, 27, 10, 0, 0, DateTimeKind.Utc);
        var notification = new OrderPaidEvent(orderId, userId, paidAt);

        await _handler.Handle(notification, CancellationToken.None);

        await _notificationService.Received(1).NotifyPaymentSuccessAsync(
            orderId, userId, paidAt, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderPaidEventReceived_SendsToCorrectUserGroup()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var paidAt = DateTime.UtcNow;
        var notification = new OrderPaidEvent(orderId, userId, paidAt);

        await _handler.Handle(notification, CancellationToken.None);

        await _notificationService.Received(1).NotifyPaymentSuccessAsync(
            orderId, userId, paidAt, Arg.Any<CancellationToken>());
        await _notificationService.DidNotReceive().NotifyPaymentSuccessAsync(
            Arg.Any<Guid>(), otherUserId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderPaidEventReceived_DoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var notification = new OrderPaidEvent(orderId, userId, DateTime.UtcNow);

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
