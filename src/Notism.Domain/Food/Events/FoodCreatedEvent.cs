using Notism.Domain.Common;
using Notism.Domain.Food.Enums;

namespace Notism.Domain.Food.Events;

public class FoodCreatedEvent : DomainEvent
{
    public Guid FoodId { get; }
    public string Name { get; }
    public FoodCategory Category { get; }

    public FoodCreatedEvent(Guid foodId, string name, FoodCategory category)
    {
        FoodId = foodId;
        Name = name;
        Category = category;
    }
}