using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Events;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Order.EventHandlers;

public class OrderCancelledRefundHandler : INotificationHandler<OrderCancelledEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<OrderCancelledRefundHandler> _logger;

    public OrderCancelledRefundHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ILogger<OrderCancelledRefundHandler> logger)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Include(o => o.Refund)
            .Where(o => o.Id == notification.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order == null || !IsEligibleForRefund(order))
        {
            return;
        }

        var refund = order.CreateRefund();
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Created pending refund {RefundId} for cancelled order {OrderId}",
            refund.Id,
            order.Id);
    }

    private static bool IsEligibleForRefund(DomainOrder order)
        => order.Refund == null
            && order.PaymentStatus == PaymentStatus.Paid
            && order.PaymentMethod == PaymentMethodEnum.Banking;
}