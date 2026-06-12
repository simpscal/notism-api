using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminOrdersForKanban;

public class AdminOrdersForKanbanHandler : IRequestHandler<AdminOrdersForKanbanRequest, AdminOrdersForKanbanResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminOrdersForKanbanHandler> _logger;

    public AdminOrdersForKanbanHandler(
        IReadDbContext readDbContext,
        ILogger<AdminOrdersForKanbanHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminOrdersForKanbanResponse> Handle(
        AdminOrdersForKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var deliveryStatus = request.Status.ToEnum<DeliveryStatus>();

        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            paymentStatus = request.PaymentStatus.ToEnum<PaymentStatus>();
        }

        System.Linq.Expressions.Expression<Func<DomainOrder, bool>> filter =
            order => order.DeliveryStatus == deliveryStatus
                && (paymentStatus == null || order.PaymentStatus == paymentStatus);

        var totalCount = await _readDbContext.BuildGraphQuery<DomainOrder>(
                filter,
                includes => includes
                    .Include(o => o.User!)
                    .Include(o => o.Items),
                query => query.OrderByDescending(o => o.CreatedAt))
            .CountAsync(cancellationToken);

        var orders = await _readDbContext.BuildGraphQuery<DomainOrder>(
                filter,
                request.Skip,
                request.Take,
                includes => includes
                    .Include(o => o.User!)
                    .Include(o => o.Items),
                query => query.OrderByDescending(o => o.CreatedAt))
            .ToListAsync(cancellationToken);

        var items = orders.Select(AdminOrdersForKanbanOrderResponse.FromDomain).ToList();

        _logger.LogInformation(
            "Retrieved {Count} orders for kanban view with status {Status}",
            items.Count,
            request.Status);

        return new AdminOrdersForKanbanResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }
}