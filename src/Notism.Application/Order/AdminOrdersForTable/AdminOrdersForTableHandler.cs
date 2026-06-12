using System.Linq.Expressions;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminOrdersForTable;

public class AdminOrdersForTableHandler : IRequestHandler<AdminOrdersForTableRequest, AdminOrdersForTableResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminOrdersForTableHandler> _logger;

    public AdminOrdersForTableHandler(
        IReadDbContext readDbContext,
        ILogger<AdminOrdersForTableHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminOrdersForTableResponse> Handle(
        AdminOrdersForTableRequest request,
        CancellationToken cancellationToken)
    {
        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            paymentStatus = request.PaymentStatus.ToEnum<PaymentStatus>();
        }

        var isDescending = (request.SortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc) == SortOrder.Desc;
        var filter = BuildFilter(request.Keyword, paymentStatus);

        IQueryable<DomainOrder> BuildQuery() =>
            _readDbContext.BuildGraphQuery<DomainOrder>(
                    filter,
                    query => ApplyOrdering(query, request.SortBy, isDescending))
                .Include(o => o.User!)
                .Include(o => o.Items);

        var totalCount = await BuildQuery().CountAsync(cancellationToken);

        var orders = await BuildQuery()
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var items = orders.Select(AdminOrdersForTableOrderResponse.FromDomain).ToList();

        _logger.LogInformation("Retrieved {Count} orders for table view", items.Count);

        return new AdminOrdersForTableResponse
        {
            TotalCount = totalCount,
            Items = items,
        };
    }

    private static IQueryable<DomainOrder> ApplyOrdering(IQueryable<DomainOrder> query, string? sortBy, bool isDescending)
    {
        return sortBy switch
        {
            "slugId" => isDescending
                ? query.OrderByDescending(o => o.SlugId)
                : query.OrderBy(o => o.SlugId),
            "totalAmount" => isDescending
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            "userName" => isDescending
                ? query.OrderByDescending(o => o.User != null ? (o.User.FirstName ?? string.Empty) + " " + (o.User.LastName ?? string.Empty) : string.Empty)
                : query.OrderBy(o => o.User != null ? (o.User.FirstName ?? string.Empty) + " " + (o.User.LastName ?? string.Empty) : string.Empty),
            "email" => isDescending
                ? query.OrderByDescending(o => o.User != null ? (string)o.User.Email : string.Empty)
                : query.OrderBy(o => o.User != null ? (string)o.User.Email : string.Empty),
            _ => query.OrderByDescending(o => o.CreatedAt),
        };
    }

    private static Expression<Func<DomainOrder, bool>> BuildFilter(string? keyword, PaymentStatus? paymentStatus)
    {
        if (string.IsNullOrWhiteSpace(keyword) && paymentStatus is null)
        {
            return order => true;
        }

        if (paymentStatus is not null && string.IsNullOrWhiteSpace(keyword))
        {
            var ps = paymentStatus.Value;
            return order => order.PaymentStatus == ps;
        }

        var keywordLower = keyword!.ToLower();
        var paymentStatusFilter = paymentStatus;

        return order =>
            (paymentStatusFilter == null || order.PaymentStatus == paymentStatusFilter) &&
            (order.SlugId.ToLower().Contains(keywordLower) ||
            (order.User != null && (
                (order.User.FirstName != null && order.User.FirstName.ToLower().Contains(keywordLower)) ||
                (order.User.LastName != null && order.User.LastName.ToLower().Contains(keywordLower)) ||
                ((string)order.User.Email).ToLower().Contains(keywordLower))));
    }
}
