using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.Event;

public interface IEventRepository : IRepository<Event>
{
    Event Update(Event eventEntity);
}