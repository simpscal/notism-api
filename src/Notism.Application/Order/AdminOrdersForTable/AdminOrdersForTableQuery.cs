using System.Linq.Expressions;

using Notism.Application.Common.Persistence;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminOrdersForTable;

/// <summary>
/// Self-contained paged read for the admin orders table: keyword filter (slug and user
/// name/email), optional payment-status filter and sort, all executed server-side. Loads
/// each order with the user and items the table row maps over; the required graph is
/// declared here and applied no-tracking in the Infrastructure read port — the Application
/// layer never calls <c>Include</c>. Owned by <see cref="AdminOrdersForTableHandler"/>;
/// every predicate, ordering and graph is duplicated inline here rather than shared with
/// any other handler.
/// </summary>
public sealed class AdminOrdersForTableQuery
{
    private readonly IReadDbContext _readDbContext;

    public AdminOrdersForTableQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<(int TotalCount, List<DomainOrder> Items)> ExecuteAsync(
        string? keyword,
        string? sortBy,
        string? sortOrder,
        PaymentStatus? paymentStatus,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var isDescending = (sortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc) == SortOrder.Desc;

        return _readDbContext.PagedWithGraphAsync<DomainOrder>(
            BuildFilter(keyword, paymentStatus),
            skip,
            take,
            includes => includes
                .Include(o => o.User!)
                .Include(o => o.Items),
            query => ApplyOrdering(query, sortBy, isDescending),
            cancellationToken);
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
