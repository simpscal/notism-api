using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Order.Events;
using Notism.Shared.Configuration;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.EventHandlers;

public class OrderPlacedHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IReadDbContext _readDbContext;
    private readonly INotifier _paymentNotifier;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<OrderPlacedHandler> _logger;

    public OrderPlacedHandler(
        IReadDbContext readDbContext,
        INotifier paymentNotifier,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        ILogger<OrderPlacedHandler> logger)
    {
        _readDbContext = readDbContext;
        _paymentNotifier = paymentNotifier;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>()
            .Where(o => o.SlugId == notification.SlugId)
            .Include(o => o.User)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(cancellationToken);

        if (order?.User == null)
        {
            _logger.LogWarning("Order-placed event for order {SlugId} skipped: order or owning user not found", notification.SlugId);
            return;
        }

        await Task.WhenAll(
            PushNotificationAsync(order, cancellationToken),
            SendEmailAsync(order));
    }

    private async Task PushNotificationAsync(DomainOrder order, CancellationToken cancellationToken)
    {
        try
        {
            await _paymentNotifier.NotifyOrderPlacedAsync(
                order.Id,
                order.SlugId,
                order.CreatedAt,
                order.TotalAmount,
                order.Items.Count,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to push order-placed notification for order {OrderId}", order.Id);
        }
    }

    private async Task SendEmailAsync(DomainOrder order)
    {
        try
        {
            await _emailService.SendNewOrderEmailAsync(
                _emailSettings.OpsRecipient,
                order.SlugId,
                order.CreatedAt,
                order.TotalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order-placed email for order {OrderId}", order.Id);
        }
    }
}