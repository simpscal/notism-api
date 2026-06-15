using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Application.Order.Common;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Shared.Exceptions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.ApproveRefund;

public class ApproveRefundHandler : IRequestHandler<ApproveRefundRequest, RefundResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<ApproveRefundHandler> _logger;
    private readonly IMessages _messages;

    public ApproveRefundHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ILogger<ApproveRefundHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<RefundResponse> Handle(ApproveRefundRequest request, CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Include(o => o.Refund)
            .Where(o => o.Refund != null && o.Refund.Id == request.RefundId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.RefundNotFound);

        if (order.Refund!.Status != RefundStatus.Pending)
        {
            throw new ConflictException(_messages.RefundNotPending);
        }

        order.MarkRefundProcessing();
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Refund {RefundId} approved and set to processing", request.RefundId);

        return RefundResponse.FromDomain(order.Refund);
    }
}