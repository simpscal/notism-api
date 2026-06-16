using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminRefundsForTable;

public class AdminRefundsForTableHandler : IRequestHandler<AdminRefundsForTableRequest, AdminRefundsForTableResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminRefundsForTableHandler> _logger;

    public AdminRefundsForTableHandler(
        IReadDbContext readDbContext,
        ILogger<AdminRefundsForTableHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminRefundsForTableResponse> Handle(
        AdminRefundsForTableRequest request,
        CancellationToken cancellationToken)
    {
        RefundStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status.ExistInEnum<RefundStatus>())
        {
            status = request.Status.ToEnum<RefundStatus>();
        }

        IQueryable<Refund> BuildQuery() =>
            _readDbContext.Set<Refund>()
                .Where(r => status == null || r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Include(r => r.Order);

        var totalCount = await BuildQuery().CountAsync(cancellationToken);

        var refunds = await BuildQuery()
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var items = refunds.Select(AdminRefundsForTableRefundResponse.FromDomain).ToList();

        _logger.LogInformation("Retrieved {Count} refunds for ledger view", items.Count);

        return new AdminRefundsForTableResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }
}