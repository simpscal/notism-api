using Notism.Domain.Order;
using Notism.Domain.Order.Repositories;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}