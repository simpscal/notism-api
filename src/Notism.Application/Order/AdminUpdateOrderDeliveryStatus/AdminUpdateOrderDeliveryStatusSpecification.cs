using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

public class AdminUpdateOrderDeliveryStatusSpecification : Specification<DomainOrder>
{
    private readonly Guid _orderId;

    public AdminUpdateOrderDeliveryStatusSpecification(Guid orderId)
    {
        _orderId = orderId;

        Include(o => o.User!);
    }

    public override Expression<Func<DomainOrder, bool>> ToExpression()
    {
        return order => order.Id == _orderId;
    }
}