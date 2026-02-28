using MediatR;

namespace Notism.Application.Food.AdminDeleteFood;

public record AdminDeleteFoodRequest : IRequest
{
    public Guid FoodId { get; init; }
}
