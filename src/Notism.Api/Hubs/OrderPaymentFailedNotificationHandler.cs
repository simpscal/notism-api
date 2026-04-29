using MediatR;

using Microsoft.AspNetCore.SignalR;

using Notism.Domain.Order.Events;

namespace Notism.Api.Hubs;

public class OrderPaymentFailedNotificationHandler : INotificationHandler<OrderPaymentFailedEvent>
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public OrderPaymentFailedNotificationHandler(IHubContext<PaymentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(OrderPaymentFailedEvent notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients
            .Group(notification.UserId.ToString())
            .SendAsync(
                "ReceivePaymentNotification",
                new
                {
                    type = "payment-failure",
                    orderId = notification.OrderId,
                    message = "Payment failed. Please try again or contact support.",
                    timestamp = notification.OccurredOn,
                },
                cancellationToken);
    }
}
