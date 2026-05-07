using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Domain.Order.Events;

namespace Notism.Application.Payment.EventHandlers;

public class OrderPaidHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly INotificationService _notificationService;

    public OrderPaidHandler(INotificationService notificationService)
        => _notificationService = notificationService;

    public Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
        => _notificationService.NotifyPaymentSuccessAsync(
            notification.OrderId, notification.UserId, notification.PaidAt, notification.SlugId, cancellationToken);
}
