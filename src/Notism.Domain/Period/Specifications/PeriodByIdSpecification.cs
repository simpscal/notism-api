using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Period.Specifications;

public class PeriodByIdSpecification : Specification<Period>
{
    private readonly Guid _periodId;

    public PeriodByIdSpecification(Guid periodId)
    {
        _periodId = periodId;
    }

    public override Expression<Func<Period, bool>> ToExpression()
    {
        return period => period.Id == _periodId && !period.IsDeleted;
    }
}

