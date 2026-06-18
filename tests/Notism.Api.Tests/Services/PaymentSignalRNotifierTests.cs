using Microsoft.AspNetCore.SignalR;

using Notism.Api.Hubs;
using Notism.Api.Services;

using NSubstitute;

namespace Notism.Api.Tests.Services;

public class PaymentSignalRNotifierTests
{
    private readonly IHubContext<PaymentHub> _hubContext;
    private readonly IHubClients _clients;
    private readonly IClientProxy _adminsGroup;
    private readonly IClientProxy _customerGroup;
    private readonly Guid _customerUserId;
    private readonly PaymentSignalRNotifier _notifier;

    public PaymentSignalRNotifierTests()
    {
        _hubContext = Substitute.For<IHubContext<PaymentHub>>();
        _clients = Substitute.For<IHubClients>();
        _adminsGroup = Substitute.For<IClientProxy>();
        _customerGroup = Substitute.For<IClientProxy>();
        _customerUserId = Guid.NewGuid();

        _hubContext.Clients.Returns(_clients);
        _clients.Group(PaymentHub.AdminsGroup).Returns(_adminsGroup);
        _clients.Group(_customerUserId.ToString()).Returns(_customerGroup);

        _notifier = new PaymentSignalRNotifier(_hubContext);
    }

    [Fact]
    public async Task NotifyRefundStatusChangedAsync_WhenPaid_NotifiesAdminsAndOwningCustomer()
    {
        var refundId = Guid.NewGuid();
        var sentDate = DateTime.UtcNow;

        await _notifier.NotifyRefundStatusChangedAsync(
            refundId, "paid", _customerUserId, "ORD-123", 485_000m, sentDate, CancellationToken.None);

        await _adminsGroup.Received(1).SendCoreAsync(
            "ReceivePaymentNotification",
            Arg.Is<object?[]>(args => MatchesStatusChanged(args, refundId, "paid")),
            Arg.Any<CancellationToken>());
        await _customerGroup.Received(1).SendCoreAsync(
            "ReceivePaymentNotification",
            Arg.Is<object?[]>(args => MatchesCustomerPaid(args, refundId, "ORD-123", 485_000m)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyRefundStatusChangedAsync_WhenFailed_NotifiesAdminsOnly()
    {
        var refundId = Guid.NewGuid();

        await _notifier.NotifyRefundStatusChangedAsync(
            refundId, "failed", _customerUserId, string.Empty, 0m, null, CancellationToken.None);

        await _adminsGroup.Received(1).SendCoreAsync(
            "ReceivePaymentNotification",
            Arg.Is<object?[]>(args => MatchesStatusChanged(args, refundId, "failed")),
            Arg.Any<CancellationToken>());
        await _customerGroup.DidNotReceive().SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    private static bool MatchesStatusChanged(object?[] args, Guid refundId, string status)
    {
        if (args.Length != 1 || args[0] is null)
        {
            return false;
        }

        var payload = args[0]!;
        var type = payload.GetType();

        return (string?)type.GetProperty("type")?.GetValue(payload) == "refund-status-changed"
            && (Guid?)type.GetProperty("refundId")?.GetValue(payload) == refundId
            && (string?)type.GetProperty("status")?.GetValue(payload) == status;
    }

    private static bool MatchesCustomerPaid(object?[] args, Guid refundId, string orderRef, decimal amount)
    {
        if (args.Length != 1 || args[0] is null)
        {
            return false;
        }

        var payload = args[0]!;
        var type = payload.GetType();

        return (string?)type.GetProperty("type")?.GetValue(payload) == "refund-status-changed"
            && (string?)type.GetProperty("status")?.GetValue(payload) == "paid"
            && (Guid?)type.GetProperty("refundId")?.GetValue(payload) == refundId
            && (string?)type.GetProperty("orderRef")?.GetValue(payload) == orderRef
            && (decimal?)type.GetProperty("amount")?.GetValue(payload) == amount;
    }
}
