using Notism.Domain.Common;

namespace Notism.Domain.Food.Events;

public class FoodCreatedEvent : DomainEvent
{
    public Guid FoodId { get; }
    public string Name { get; }
    public Guid? CategoryId { get; }

    public FoodCreatedEvent(Guid foodId, string name, Guid? categoryId)
    {
        FoodId = foodId;
        Name = name;
        CategoryId = categoryId;
    }
}