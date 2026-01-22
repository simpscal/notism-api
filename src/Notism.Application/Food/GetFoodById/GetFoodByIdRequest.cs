using MediatR;

namespace Notism.Application.Food.GetFoodById;

public class GetFoodByIdRequest : IRequest<GetFoodByIdResponse>
{
    public Guid FoodId { get; set; }
}