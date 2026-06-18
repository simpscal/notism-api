using Microsoft.AspNetCore.SignalR;

using Notism.Api.Hubs;
using Notism.Application.Common.Services;

namespace Notism.Api.Services;

public class PaymentSignalRNotifier : IPaymentNotifier
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public PaymentSignalRNotifier(IHubContext<PaymentHub> hubContext)
        => _hubContext = hubContext;

    public Task NotifyAdminRefundStatusChangedAsync(Guid refundId, string status, CancellationToken cancellationToken)
        => _hubContext.Clients.Group(PaymentHub.AdminsGroup).SendAsync(
            "ReceivePaymentNotification",
            new { type = "refund-status-changed", refundId, status, timestamp = DateTime.UtcNow },
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

    public Task NotifyRefundPaidAsync(Guid refundId, string orderId, string orderRef, decimal amount, DateTime sentDate, Guid userId, CancellationToken cancellationToken)
        => _hubContext.Clients.Group(userId.ToString()).SendAsync(
            "ReceivePaymentNotification",
            new { type = "refund-paid", refundId, orderId, orderRef, amount, message = $"Your refund for order {orderRef} has been sent.", timestamp = sentDate },
            cancellationToken);
}