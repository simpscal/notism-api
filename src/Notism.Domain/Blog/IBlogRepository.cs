using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.Blog;

public interface IBlogRepository : IRepository<Blog>
{
    Blog Update(Blog blog);
}