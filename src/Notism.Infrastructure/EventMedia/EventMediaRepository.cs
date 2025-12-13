using Notism.Domain.EventMedia;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.EventMedia;

public class EventMediaRepository : Repository<Domain.EventMedia.EventMedia>, IEventMediaRepository
{
    public EventMediaRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }
}