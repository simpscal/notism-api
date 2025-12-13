using Notism.Domain.ContentVersion;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.ContentVersions;

public class ContentVersionRepository : Repository<ContentVersion>, IContentVersionRepository
{
    public ContentVersionRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}