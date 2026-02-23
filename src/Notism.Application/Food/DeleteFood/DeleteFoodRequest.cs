using MediatR;

namespace Notism.Application.Food.DeleteFood;

public record DeleteFoodRequest : IRequest
{
    public Guid FoodId { get; init; }
}
