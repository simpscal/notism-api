using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Event.Specifications;

public class PublishedEventsByPeriodIdSpecification : Specification<Event>
{
    private readonly Guid _periodId;

    public PublishedEventsByPeriodIdSpecification(Guid periodId)
    {
        _periodId = periodId;
    }

    public override Expression<Func<Event, bool>> ToExpression()
    {
        return e => e.PeriodId == _periodId && e.IsPublished && !e.IsDeleted;
    }
}

