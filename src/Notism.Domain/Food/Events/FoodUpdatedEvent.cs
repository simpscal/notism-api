using Notism.Domain.Common;

namespace Notism.Domain.Food.Events;

public class FoodUpdatedEvent : DomainEvent
{
    public Guid FoodId { get; }
    public string Name { get; }
    public Guid? CategoryId { get; }

    public FoodUpdatedEvent(Guid foodId, string name, Guid? categoryId)
    {
        FoodId = foodId;
        Name = name;
        CategoryId = categoryId;
    }
}