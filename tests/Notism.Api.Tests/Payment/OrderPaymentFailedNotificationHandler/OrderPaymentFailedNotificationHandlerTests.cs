using FluentAssertions;

using Microsoft.AspNetCore.SignalR;

using Notism.Api.Hubs;
using Notism.Domain.Order.Events;

using NSubstitute;

namespace Notism.Api.Tests.Payment.OrderPaymentFailedNotificationHandler;

public class OrderPaymentFailedNotificationHandlerTests
{
    private readonly IHubContext<PaymentHub> _hubContext;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _clientProxy;
    private readonly Hubs.OrderPaymentFailedNotificationHandler _handler;

    public OrderPaymentFailedNotificationHandlerTests()
    {
        _hubContext = Substitute.For<IHubContext<PaymentHub>>();
        _hubClients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _handler = new Hubs.OrderPaymentFailedNotificationHandler(_hubContext);
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentFailedEventReceived_SendsPaymentFailureToUsersGroup()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var notification = new OrderPaymentFailedEvent(orderId, userId);

        await _handler.Handle(notification, CancellationToken.None);

        _hubClients.Received(1).Group(userId.ToString());
        await _clientProxy.Received(1).SendCoreAsync(
            "ReceivePaymentNotification",
            Arg.Is<object[]>(args => args.Length == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderPaymentFailedEventReceived_SendsToCorrectUserGroup()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var notification = new OrderPaymentFailedEvent(orderId, userId);

        await _handler.Handle(notification, CancellationToken.None);

        _hubClients.Received(1).Group(userId.ToString());
        _hubClients.DidNotReceive().Group(otherUserId.ToString());
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
