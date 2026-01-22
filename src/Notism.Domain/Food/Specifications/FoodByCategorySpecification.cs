using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Food.Enums;

namespace Notism.Domain.Food.Specifications;

public class FoodByCategorySpecification : Specification<Food>
{
    private readonly FoodCategory _category;

    public FoodByCategorySpecification(FoodCategory category)
    {
        _category = category;
    }

    public override Expression<Func<Food, bool>> ToExpression()
    {
        return food => food.Category == _category;
    }
}