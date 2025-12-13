using Notism.Domain.Period;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Periods;

public class PeriodRepository : Repository<Period>, IPeriodRepository
{
    public PeriodRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }

    public Period Update(Period period)
    {
        _dbSet.Update(period);
        return period;
    }
}