using Notism.Domain.Blog;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Blogs;

public class BlogRepository : Repository<Blog>, IBlogRepository
{
    public BlogRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }

    public Blog Update(Blog blog)
    {
        _dbSet.Update(blog);
        return blog;
    }
}