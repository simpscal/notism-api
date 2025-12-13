using Notism.Domain.BlogMedia;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.BlogMedia;

public class BlogMediaRepository : Repository<Domain.BlogMedia.BlogMedia>, IBlogMediaRepository
{
    public BlogMediaRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}