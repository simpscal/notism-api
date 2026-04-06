using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminOrdersForKanban;

public class AdminOrdersForKanbanSpecification : Specification<DomainOrder>
{
    private readonly DeliveryStatus _deliveryStatus;
    private readonly PaymentStatus? _paymentStatus;

    public AdminOrdersForKanbanSpecification(DeliveryStatus deliveryStatus, PaymentStatus? paymentStatus = null)
    {
        _deliveryStatus = deliveryStatus;
        _paymentStatus = paymentStatus;

        Include(o => o.User!);
        Include(o => o.Items);
    }

    public override Expression<Func<DomainOrder, bool>> ToExpression()
    {
        var ps = _paymentStatus;
        return order => order.DeliveryStatus == _deliveryStatus && (ps == null || order.PaymentStatus == ps);
    }

    public override IQueryable<DomainOrder> ApplyOrdering(IQueryable<DomainOrder> queryable)
    {
        return queryable.OrderByDescending(o => o.CreatedAt);
    }
}