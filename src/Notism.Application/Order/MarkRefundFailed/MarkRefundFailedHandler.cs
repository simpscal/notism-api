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

namespace Notism.Application.Order.MarkRefundFailed;

public class MarkRefundFailedHandler : IRequestHandler<MarkRefundFailedRequest, RefundResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<MarkRefundFailedHandler> _logger;
    private readonly IMessages _messages;

    public MarkRefundFailedHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ILogger<MarkRefundFailedHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<RefundResponse> Handle(MarkRefundFailedRequest request, CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Include(o => o.Refund)
            .Where(o => o.Refund != null && o.Refund.Id == request.RefundId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.RefundNotFound);

        if (order.Refund!.Status != RefundStatus.Processing)
        {
            throw new ConflictException(_messages.RefundNotProcessing);
        }

        order.MarkRefundFailed(request.Reason);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Refund {RefundId} marked as failed", request.RefundId);

        return RefundResponse.FromDomain(order.Refund);
    }
}