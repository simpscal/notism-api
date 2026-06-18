using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Application.Order.Common;
using Notism.Domain.Order.Repositories;
using Notism.Shared.Exceptions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.RequestRefund;

public class RequestRefundHandler : IRequestHandler<RequestRefundRequest, OrderRefundResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<RequestRefundHandler> _logger;
    private readonly IMessages _messages;

    public RequestRefundHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ILogger<RequestRefundHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<OrderRefundResponse> Handle(
        RequestRefundRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
                .Where(o => o.Id == request.OrderId && o.UserId == request.UserId)
                .Include(o => o.StatusHistory)
                .Include(o => o.Refund)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

        if (order.Refund != null)
        {
            throw new ConflictException(_messages.RefundAlreadyRequested);
        }

        if (!order.IsRefundRequestEligible(DateTime.UtcNow))
        {
            throw new ResultFailureException(_messages.RefundNotEligible);
        }

        var refund = order.RequestRefund();
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Customer {UserId} requested refund {RefundId} for order {OrderId}",
            request.UserId,
            refund.Id,
            request.OrderId);

        return OrderRefundResponse.FromDomain(refund);
    }
}