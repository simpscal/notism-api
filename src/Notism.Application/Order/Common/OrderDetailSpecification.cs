using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.Common;

/// <summary>
/// Loads orders with the graph required to build an order response (items, each
/// item's food and food images, and the order's status history). Encapsulates
/// the shared include chain reused by the order read handlers so it is defined
/// in exactly one place.
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
}