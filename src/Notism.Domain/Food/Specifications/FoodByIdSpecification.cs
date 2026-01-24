using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Food.Specifications;

public class FoodByIdSpecification : Specification<Food>
{
    private readonly Guid _foodId;

    public FoodByIdSpecification(Guid foodId)
    {
        _foodId = foodId;
        Include(food => food.Images);
    }

    public override Expression<Func<Food, bool>> ToExpression()
    {
        return food => food.Id == _foodId;
    }
}