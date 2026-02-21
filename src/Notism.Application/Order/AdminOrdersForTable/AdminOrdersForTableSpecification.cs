using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminOrdersForTable;

public class AdminOrdersForTableSpecification : Specification<DomainOrder>
{
    private readonly string? _keyword;
    private readonly string? _sortBy;
    private readonly bool _isDescending;

    public AdminOrdersForTableSpecification(
        string? keyword = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        _keyword = keyword;
        _sortBy = sortBy;
        var sortOrderEnum = sortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc;
        _isDescending = sortOrderEnum == SortOrder.Desc;

        Include(o => o.User!);
        Include(o => o.StatusHistory);
    }

    public override Expression<Func<DomainOrder, bool>> ToExpression()
    {
        if (string.IsNullOrWhiteSpace(_keyword))
        {
            return order => true;
        }

        var keywordLower = _keyword!.ToLower();

        return order =>
            order.SlugId.ToLower().Contains(keywordLower) ||
            (order.User != null && (
                (order.User.FirstName != null && order.User.FirstName.ToLower().Contains(keywordLower)) ||
                (order.User.LastName != null && order.User.LastName.ToLower().Contains(keywordLower)) ||
                ((string)order.User.Email).ToLower().Contains(keywordLower)));
    }

    public override IQueryable<DomainOrder> ApplyOrdering(IQueryable<DomainOrder> queryable)
    {
        return _sortBy?.ToLower() switch
        {
            "slugId" => _isDescending
                ? queryable.OrderByDescending(o => o.SlugId)
                : queryable.OrderBy(o => o.SlugId),
            "totalAmount" => _isDescending
                ? queryable.OrderByDescending(o => o.TotalAmount)
                : queryable.OrderBy(o => o.TotalAmount),
            _ => queryable.OrderByDescending(o => o.CreatedAt),
        };
    }
}