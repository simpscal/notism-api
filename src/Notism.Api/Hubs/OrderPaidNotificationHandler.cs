using MediatR;

using Microsoft.AspNetCore.SignalR;

using Notism.Domain.Order.Events;

namespace Notism.Api.Hubs;

public class OrderPaidNotificationHandler : INotificationHandler<OrderPaidEvent>
{
    private readonly IHubContext<PaymentHub> _hubContext;

    public OrderPaidNotificationHandler(IHubContext<PaymentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients
            .Group(notification.UserId.ToString())
            .SendAsync(
                "ReceivePaymentNotification",
                new
                {
                    type = "payment-success",
                    orderId = notification.OrderId,
                    message = "Payment received! Your order is being processed.",
                    timestamp = notification.OccurredOn,
                },
                cancellationToken);
    }
}
