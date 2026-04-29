using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Order.Events;

namespace Notism.Application.Payment.EventHandlers;

public class OrderPaymentFailedHandler : INotificationHandler<OrderPaymentFailedEvent>
{
    private readonly INotificationService _notificationService;

    public OrderPaymentFailedHandler(INotificationService notificationService)
        => _notificationService = notificationService;

    public Task Handle(OrderPaymentFailedEvent notification, CancellationToken cancellationToken)
        => _notificationService.NotifyPaymentFailureAsync(
            notification.OrderId, notification.UserId, cancellationToken);
}
