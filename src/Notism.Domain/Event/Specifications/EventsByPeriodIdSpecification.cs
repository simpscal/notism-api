using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Event.Specifications;

public class EventsByPeriodIdSpecification : Specification<Event>
{
    private readonly Guid _periodId;

    public EventsByPeriodIdSpecification(Guid periodId)
    {
        _periodId = periodId;
    }

    public override Expression<Func<Event, bool>> ToExpression()
    {
        return e => e.PeriodId == _periodId && !e.IsDeleted;
    }
}

