using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.GetRefundById;

public class GetRefundByIdHandler : IRequestHandler<GetRefundByIdRequest, GetRefundByIdResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<GetRefundByIdHandler> _logger;
    private readonly IMessages _messages;

    public GetRefundByIdHandler(
        IReadDbContext readDbContext,
        ILogger<GetRefundByIdHandler> logger,
        IMessages messages)
    {
        _readDbContext = readDbContext;
        _logger = logger;
        _messages = messages;
    }

    public async Task<GetRefundByIdResponse> Handle(
        GetRefundByIdRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _readDbContext.Set<DomainOrder>()
                .Where(o => o.Refund != null && o.Refund.Id == request.RefundId)
                .Include(o => o.User!)
                .Include(o => o.Refund)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(_messages.RefundNotFound);

        _logger.LogInformation("Retrieved refund {RefundId} for admin detail", request.RefundId);

        return GetRefundByIdResponse.FromDomain(order);
    }
}