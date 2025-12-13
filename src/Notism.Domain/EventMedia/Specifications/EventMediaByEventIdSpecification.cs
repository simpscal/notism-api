using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.EventMedia.Specifications;

public class EventMediaByEventIdSpecification : Specification<EventMedia>
{
    private readonly Guid _eventId;

    public EventMediaByEventIdSpecification(Guid eventId)
    {
        _eventId = eventId;
    }

    public override Expression<Func<EventMedia, bool>> ToExpression()
    {
        return em => em.EventId == _eventId && !em.IsDeleted;
    }
}

