using Notism.Domain.Event;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Events;

public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }

    public Event Update(Event eventEntity)
    {
        _dbSet.Update(eventEntity);
        return eventEntity;
    }
}