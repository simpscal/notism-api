using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Event.Specifications;

public class EventByIdSpecification : Specification<Event>
{
    private readonly Guid _eventId;

    public EventByIdSpecification(Guid eventId)
    {
        _eventId = eventId;
    }

    public override Expression<Func<Event, bool>> ToExpression()
    {
        return e => e.Id == _eventId && !e.IsDeleted;
    }
}

