using Notism.Domain.BlogEventMention;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.BlogEventMentions;

public class BlogEventMentionRepository : Repository<BlogEventMention>, IBlogEventMentionRepository
{
    public BlogEventMentionRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}