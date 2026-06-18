using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Order.Events;

namespace Notism.Application.Order.EventHandlers;

public class RefundFailedHandler : INotificationHandler<RefundFailedEvent>
{
    private readonly IPaymentNotifier _paymentNotifier;
    private readonly ILogger<RefundFailedHandler> _logger;

    public RefundFailedHandler(
        IPaymentNotifier paymentNotifier,
        ILogger<RefundFailedHandler> logger)
    {
        _paymentNotifier = paymentNotifier;
        _logger = logger;
    }

    public async Task Handle(RefundFailedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _paymentNotifier.NotifyAdminRefundStatusChangedAsync(notification.RefundId, "failed", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push admin refund-status-changed notification for refund {RefundId}", notification.RefundId);
        }
    }
}