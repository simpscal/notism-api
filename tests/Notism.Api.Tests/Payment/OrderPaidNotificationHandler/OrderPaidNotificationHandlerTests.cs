using FluentAssertions;

using Microsoft.AspNetCore.SignalR;

using Notism.Api.Hubs;
using Notism.Domain.Order.Events;

using NSubstitute;

namespace Notism.Api.Tests.Payment.OrderPaidNotificationHandler;

public class OrderPaidNotificationHandlerTests
{
    private readonly IHubContext<PaymentHub> _hubContext;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _clientProxy;
    private readonly Hubs.OrderPaidNotificationHandler _handler;

    public OrderPaidNotificationHandlerTests()
    {
        _hubContext = Substitute.For<IHubContext<PaymentHub>>();
        _hubClients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _handler = new Hubs.OrderPaidNotificationHandler(_hubContext);
    }

    [Fact]
    public async Task Handle_WhenOrderPaidEventReceived_SendsPaymentSuccessToUsersGroup()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var paidAt = new DateTime(2026, 4, 27, 10, 0, 0, DateTimeKind.Utc);
        var notification = new OrderPaidEvent(orderId, userId, paidAt);

        await _handler.Handle(notification, CancellationToken.None);

        _hubClients.Received(1).Group(userId.ToString());
        await _clientProxy.Received(1).SendCoreAsync(
            "ReceivePaymentNotification",
            Arg.Is<object[]>(args => args.Length == 1),
            Arg.Any<CancellationToken>());
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

        _hubClients.Received(1).Group(userId.ToString());
        _hubClients.DidNotReceive().Group(otherUserId.ToString());
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
