using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Food.Specifications;

public class AvailableFoodsSpecification : Specification<Food>
{
    public override Expression<Func<Food, bool>> ToExpression()
    {
        return food => food.IsAvailable && food.StockQuantity > 0;
    }
}