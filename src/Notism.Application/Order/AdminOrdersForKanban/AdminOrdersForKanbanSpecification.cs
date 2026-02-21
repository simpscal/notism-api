using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Order.Enums;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminOrdersForKanban;

public class AdminOrdersForKanbanSpecification : Specification<DomainOrder>
{
    private readonly DeliveryStatus _deliveryStatus;

    public AdminOrdersForKanbanSpecification(DeliveryStatus deliveryStatus)
    {
        _deliveryStatus = deliveryStatus;

        Include(o => o.User!);
        Include(o => o.Items);
    }

    public override Expression<Func<DomainOrder, bool>> ToExpression()
    {
        return order => order.DeliveryStatus == _deliveryStatus;
    }

    public override IQueryable<DomainOrder> ApplyOrdering(IQueryable<DomainOrder> queryable)
    {
        return queryable.OrderByDescending(o => o.CreatedAt);
    }
}