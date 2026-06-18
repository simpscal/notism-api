using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.EventHandlers;
using Notism.Domain.Order.Events;

using NSubstitute;

namespace Notism.Application.Tests.Order.EventHandlers;

public class RefundFailedHandlerTests
{
    private readonly IPaymentNotifier _paymentNotifier;
    private readonly RefundFailedHandler _handler;

    public RefundFailedHandlerTests()
    {
        _paymentNotifier = Substitute.For<IPaymentNotifier>();
        _handler = new RefundFailedHandler(
            _paymentNotifier,
            Substitute.For<ILogger<RefundFailedHandler>>());
    }

    [Fact]
    public async Task Handle_WhenRefundFailed_NotifiesAdminsRefundStatusFailed()
    {
        var notification = new RefundFailedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "bank rejected");

        await _handler.Handle(notification, CancellationToken.None);

        await _paymentNotifier.Received(1).NotifyAdminRefundStatusChangedAsync(
            notification.RefundId,
            "failed",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAdminNotifyThrows_DoesNotRethrow()
    {
        var notification = new RefundFailedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "bank rejected");
        _paymentNotifier
            .NotifyAdminRefundStatusChangedAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("hub down")));

        var act = async () => await _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}