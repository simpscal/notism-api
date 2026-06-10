using MediatR;

using Notism.Application.Common.Services;
using Notism.Domain.Order.Events;

namespace Notism.Application.Payment.EventHandlers;

public class OrderPaymentFailedHandler : INotificationHandler<OrderPaymentFailedEvent>
{
    private readonly IPaymentNotifier _paymentNotifier;

    public OrderPaymentFailedHandler(IPaymentNotifier paymentNotifier)
        => _paymentNotifier = paymentNotifier;

    public Task Handle(OrderPaymentFailedEvent notification, CancellationToken cancellationToken)
        => _paymentNotifier.NotifyPaymentFailureAsync(
            notification.OrderId, notification.UserId, cancellationToken);
}