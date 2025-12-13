using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Blog.Specifications;

public class BlogBySlugSpecification : Specification<Blog>
{
    private readonly string _slug;

    public BlogBySlugSpecification(string slug)
    {
        _slug = slug;
    }

    public override Expression<Func<Blog, bool>> ToExpression()
    {
        return blog => blog.Slug == _slug && !blog.IsDeleted;
    }
}

