using Microsoft.AspNetCore.SignalR;

using Notism.Api.Hubs;
using Notism.Application.Common.Services;

namespace Notism.Api.Services;

public class PaymentSignalRNotifier : IPaymentNotifier
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public PaymentSignalRNotifier(IHubContext<PaymentHub> hubContext)
        => _hubContext = hubContext;

    public async Task NotifyRefundStatusChangedAsync(Guid refundId, string status, Guid customerUserId, string orderRef, decimal amount, DateTime? sentDate, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(PaymentHub.AdminsGroup).SendAsync(
            "ReceivePaymentNotification",
            new { type = "refund-status-changed", refundId, status, timestamp = DateTime.UtcNow },
            cancellationToken);

        if (status == "paid")
        {
            await _hubContext.Clients.Group(customerUserId.ToString()).SendAsync(
                "ReceivePaymentNotification",
                new { type = "refund-status-changed", status, refundId, orderId = orderRef, orderRef, amount, message = $"Your refund for order {orderRef} has been sent.", timestamp = sentDate ?? DateTime.UtcNow },
                cancellationToken);
        }
    }

    public Task NotifyOrderPlacedAsync(Guid orderId, string orderNumber, DateTime placedAt, decimal total, int itemCount, CancellationToken cancellationToken)
        => _hubContext.Clients.Group(PaymentHub.AdminsGroup).SendAsync(
            "ReceivePaymentNotification",
            new { type = "order-placed", orderId, orderNumber, placedAt, total, itemCount, timestamp = DateTime.UtcNow },
            cancellationToken);

    public Task NotifyPaymentSuccessAsync(Guid orderId, Guid userId, DateTime paidAt, string slugId, CancellationToken cancellationToken)
        => _hubContext.Clients.Group(userId.ToString()).SendAsync(
            "ReceivePaymentNotification",
            new { type = "payment-success", orderId, slugId, message = "Payment confirmed. You can now view your order.", timestamp = paidAt },
            cancellationToken);

    public Task NotifyPaymentFailureAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
        => _hubContext.Clients.Group(userId.ToString()).SendAsync(
            "ReceivePaymentNotification",
            new { type = "payment-failure", orderId, message = "Payment failed. Please try again or contact support.", timestamp = DateTime.UtcNow },
            cancellationToken);
}