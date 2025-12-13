using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.BlogMedia.Specifications;

public class BlogMediaByBlogIdSpecification : Specification<BlogMedia>
{
    private readonly Guid _blogId;

    public BlogMediaByBlogIdSpecification(Guid blogId)
    {
        _blogId = blogId;
    }

    public override Expression<Func<BlogMedia, bool>> ToExpression()
    {
        return bm => bm.BlogId == _blogId && !bm.IsDeleted;
    }
}

