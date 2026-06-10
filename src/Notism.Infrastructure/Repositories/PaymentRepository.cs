using Notism.Domain.Payment;
using Notism.Domain.Payment.Repositories;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class PaymentRepository : Repository<Domain.Payment.Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}