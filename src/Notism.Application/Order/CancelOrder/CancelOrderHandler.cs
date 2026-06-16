using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Exceptions;

using DomainOrder = Notism.Domain.Order.Order;
using PaymentMethodEnum = Notism.Domain.Order.Enums.PaymentMethod;

namespace Notism.Application.Order.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderRequest>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<CancelOrderHandler> _logger;
    private readonly IMessages _messages;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ILogger<CancelOrderHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(
        CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
                .Include(o => o.Refund)
                .Where(o => o.Id == request.OrderId && o.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new ResultFailureException(ex.Message);
        }

        if (IsEligibleForRefund(order))
        {
            order.CreateRefund();
        }

        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Cancelled order {OrderId} for user {UserId}",
            request.OrderId,
            request.UserId);
    }

    private static bool IsEligibleForRefund(DomainOrder order)
        => order.Refund == null
            && order.PaymentStatus == PaymentStatus.Paid
            && order.PaymentMethod == PaymentMethodEnum.Banking;
}