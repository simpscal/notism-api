using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.BlogEventMention.Specifications;

public class BlogEventMentionByEventIdSpecification : Specification<BlogEventMention>
{
    private readonly Guid _eventId;

    public BlogEventMentionByEventIdSpecification(Guid eventId)
    {
        _eventId = eventId;
    }

    public override Expression<Func<BlogEventMention, bool>> ToExpression()
    {
        return bem => bem.EventId == _eventId && !bem.IsDeleted;
    }
}

