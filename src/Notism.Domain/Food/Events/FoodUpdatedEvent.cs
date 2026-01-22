using Notism.Domain.Common;
using Notism.Domain.Food.Enums;

namespace Notism.Domain.Food.Events;

public class FoodUpdatedEvent : DomainEvent
{
    public Guid FoodId { get; }
    public string Name { get; }
    public FoodCategory Category { get; }

    public FoodUpdatedEvent(Guid foodId, string name, FoodCategory category)
    {
        FoodId = foodId;
        Name = name;
        Category = category;
    }
}