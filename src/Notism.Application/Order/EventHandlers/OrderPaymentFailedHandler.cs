using MediatR;

using Notism.Application.Common.Services;
using Notism.Domain.Order.Events;

namespace Notism.Application.Order.EventHandlers;

public class OrderPaymentFailedHandler : INotificationHandler<OrderPaymentFailedEvent>
{
    private readonly INotifier _paymentNotifier;

    public OrderPaymentFailedHandler(INotifier paymentNotifier)
        => _paymentNotifier = paymentNotifier;

    public Task Handle(OrderPaymentFailedEvent notification, CancellationToken cancellationToken)
        => _paymentNotifier.NotifyPaymentFailureAsync(
            notification.OrderId, notification.UserId, cancellationToken);
}