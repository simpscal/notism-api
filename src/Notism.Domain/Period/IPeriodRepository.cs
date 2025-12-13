using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.Period;

public interface IPeriodRepository : IRepository<Period>
{
    Period Update(Period period);
}