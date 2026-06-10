using Notism.Domain.Payment;
using Notism.Domain.Payment.Repositories;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class BankingCheckoutRepository : Repository<BankingCheckout>, IBankingCheckoutRepository
{
    public BankingCheckoutRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}