using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Order.Events;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.EventHandlers;

public class RefundPaidHandler : INotificationHandler<RefundPaidEvent>
{
    private readonly IReadDbContext _readDbContext;
    private readonly INotifier _paymentNotifier;
    private readonly IEmailService _emailService;
    private readonly ILogger<RefundPaidHandler> _logger;

    public RefundPaidHandler(
        IReadDbContext readDbContext,
        INotifier paymentNotifier,
        IEmailService emailService,
        ILogger<RefundPaidHandler> logger)
    {
        _readDbContext = readDbContext;
        _paymentNotifier = paymentNotifier;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(RefundPaidEvent notification, CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>()
            .Where(o => o.Id == notification.OrderId)
            .Include(o => o.User)
            .Include(o => o.Refund)
            .FirstOrDefaultAsync(cancellationToken);

        if (order?.Refund == null)
        {
            _logger.LogWarning("Refund-paid event for order {OrderId} skipped: order or refund not found", notification.OrderId);
            return;
        }

        await Task.WhenAll(
            PushNotificationAsync(order, notification, cancellationToken),
            SendEmailAsync(order, notification));
    }

    private async Task PushNotificationAsync(DomainOrder order, RefundPaidEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _paymentNotifier.NotifyRefundStatusChangedAsync(
                notification.RefundId,
                "paid",
                notification.UserId,
                order.SlugId,
                order.Refund!.Amount,
                notification.PaidAt,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push refund-paid notification for order {OrderId}", order.Id);
        }
    }

    private async Task SendEmailAsync(DomainOrder order, RefundPaidEvent notification)
    {
        if (order.User == null)
        {
            _logger.LogWarning("Refund-paid email for order {OrderId} skipped: owning user not found", order.Id);
            return;
        }

        try
        {
            await _emailService.SendRefundPaidEmailAsync(
                order.User.Email,
                order.User.FirstName,
                order.SlugId,
                order.Refund!.Amount,
                notification.TransferReference,
                notification.PaidAt,
                order.SlugId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send refund-paid email for order {OrderId}", order.Id);
        }
    }
}