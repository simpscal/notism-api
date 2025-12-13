using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Blog.Specifications;

public class BlogByIdSpecification : Specification<Blog>
{
    private readonly Guid _blogId;

    public BlogByIdSpecification(Guid blogId)
    {
        _blogId = blogId;
    }

    public override Expression<Func<Blog, bool>> ToExpression()
    {
        return blog => blog.Id == _blogId && !blog.IsDeleted;
    }
}

