using Notism.Domain.Order;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Orders;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}