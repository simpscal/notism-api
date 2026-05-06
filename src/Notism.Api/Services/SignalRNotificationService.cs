using Microsoft.AspNetCore.SignalR;

using Notism.Api.Hubs;
using Notism.Application.Common.Interfaces;

namespace Notism.Api.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public SignalRNotificationService(IHubContext<PaymentHub> hubContext)
        => _hubContext = hubContext;

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
