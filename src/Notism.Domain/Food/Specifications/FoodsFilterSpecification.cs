using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Food.Enums;

namespace Notism.Domain.Food.Specifications;

public class FoodsFilterSpecification : Specification<Food>
{
    private readonly FoodCategory? _category;
    private readonly string? _keyword;
    private readonly bool? _isAvailable;

    public FoodsFilterSpecification(
        FoodCategory? category = null,
        string? keyword = null,
        bool? isAvailable = null)
    {
        _category = category;
        _keyword = keyword?.ToLower();
        _isAvailable = isAvailable;
        Include(food => food.Images);
    }

    public override Expression<Func<Food, bool>> ToExpression()
    {
        return food =>
            !food.IsDeleted &&
            (!_category.HasValue || food.Category == _category.Value) &&
            (!_isAvailable.HasValue || food.IsAvailable == _isAvailable.Value) &&
            (string.IsNullOrWhiteSpace(_keyword) ||
                food.Name.ToLower().Contains(_keyword) ||
                food.Description.ToLower().Contains(_keyword));
    }

    public override IQueryable<Food> ApplyOrdering(IQueryable<Food> queryable)
    {
        return queryable.OrderByDescending(f => f.CreatedAt);
    }
}