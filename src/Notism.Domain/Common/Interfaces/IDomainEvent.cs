namespace Notism.Domain.Common.Interfaces;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}