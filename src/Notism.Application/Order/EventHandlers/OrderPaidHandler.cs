using MediatR;

using Notism.Application.Common.Services;
using Notism.Domain.Order.Events;

namespace Notism.Application.Order.EventHandlers;

public class OrderPaidHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly INotifier _paymentNotifier;

    public OrderPaidHandler(INotifier paymentNotifier)
        => _paymentNotifier = paymentNotifier;

    public Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
        => _paymentNotifier.NotifyPaymentSuccessAsync(
            notification.OrderId, notification.UserId, notification.PaidAt, notification.SlugId, cancellationToken);
}