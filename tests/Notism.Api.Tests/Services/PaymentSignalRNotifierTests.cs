using FluentAssertions;

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
    private readonly PaymentSignalRNotifier _notifier;

    public PaymentSignalRNotifierTests()
    {
        _hubContext = Substitute.For<IHubContext<PaymentHub>>();
        _clients = Substitute.For<IHubClients>();
        _adminsGroup = Substitute.For<IClientProxy>();

        _hubContext.Clients.Returns(_clients);
        _clients.Group(PaymentHub.AdminsGroup).Returns(_adminsGroup);

        _notifier = new PaymentSignalRNotifier(_hubContext);
    }

    [Fact]
    public async Task NotifyAdminRefundStatusChangedAsync_WhenPaid_SendsRefundStatusChangedToAdminsGroup()
    {
        var refundId = Guid.NewGuid();

        await _notifier.NotifyAdminRefundStatusChangedAsync(refundId, "paid", CancellationToken.None);

        _clients.Received(1).Group(PaymentHub.AdminsGroup);
        await _adminsGroup.Received(1).SendCoreAsync(
            "ReceivePaymentNotification",
            Arg.Is<object?[]>(args => MatchesPayload(args, refundId, "paid")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotifyAdminRefundStatusChangedAsync_WhenFailed_SendsRefundStatusChangedToAdminsGroup()
    {
        var refundId = Guid.NewGuid();

        await _notifier.NotifyAdminRefundStatusChangedAsync(refundId, "failed", CancellationToken.None);

        _clients.Received(1).Group(PaymentHub.AdminsGroup);
        await _adminsGroup.Received(1).SendCoreAsync(
            "ReceivePaymentNotification",
            Arg.Is<object?[]>(args => MatchesPayload(args, refundId, "failed")),
            Arg.Any<CancellationToken>());
    }

    private static bool MatchesPayload(object?[] args, Guid refundId, string status)
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
}