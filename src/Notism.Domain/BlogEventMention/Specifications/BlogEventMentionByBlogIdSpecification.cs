using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.BlogEventMention.Specifications;

public class BlogEventMentionByBlogIdSpecification : Specification<BlogEventMention>
{
    private readonly Guid _blogId;

    public BlogEventMentionByBlogIdSpecification(Guid blogId)
    {
        _blogId = blogId;
    }

    public override Expression<Func<BlogEventMention, bool>> ToExpression()
    {
        return bem => bem.BlogId == _blogId && !bem.IsDeleted;
    }
}

