using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.Common;

/// <summary>
/// Loads orders with the graph required to build an order response (items, each
/// item's food and food images, and the order's status history). Encapsulates
/// the shared include chain reused by the order read handlers so it is defined
/// in exactly one place.
///
/// Owns the deterministic ordering used by the paged orders read: most recent
/// first by <c>CreatedAt</c> with the primary key as a tie-breaker, so no row is
/// duplicated or skipped across page boundaries. Callers compose the full filter
/// (including any payment-status condition) into the expression they pass in, so
/// it runs at the database level and paged counts stay correct.
/// </summary>
public class OrderDetailSpecification : Specification<DomainOrder>
{
    private readonly Expression<Func<DomainOrder, bool>> _filter;

    public OrderDetailSpecification(Expression<Func<DomainOrder, bool>> filter)
    {
        _filter = filter;

        Include("Items.Food.Images");
        Include(o => o.StatusHistory);
    }

    public override Expression<Func<DomainOrder, bool>> ToExpression()
    {
        return _filter;
    }

    public override IQueryable<DomainOrder> ApplyOrdering(IQueryable<DomainOrder> queryable)
    {
        return queryable
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id);
    }
}