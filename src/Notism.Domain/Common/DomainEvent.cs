using Notism.Domain.Common.Events;

namespace Notism.Domain.Common;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}