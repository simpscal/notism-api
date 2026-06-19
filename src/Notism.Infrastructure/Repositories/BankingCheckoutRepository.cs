using Notism.Domain.Order;
using Notism.Domain.Order.Repositories;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class BankingCheckoutRepository : Repository<BankingCheckout>, IBankingCheckoutRepository
{
    public BankingCheckoutRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}