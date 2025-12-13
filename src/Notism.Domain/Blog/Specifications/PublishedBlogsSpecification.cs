using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Blog.Specifications;

public class PublishedBlogsSpecification : Specification<Blog>
{
    public override Expression<Func<Blog, bool>> ToExpression()
    {
        return blog => blog.IsPublished && !blog.IsDeleted && blog.PublishedAt != null;
    }
}

