using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Food.Enums;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsSpecification : Specification<Domain.Food.Food>
{
    private readonly FoodCategory? _category;
    private readonly string? _keyword;
    private readonly bool? _isAvailable;

    public GetFoodsSpecification(
        FoodCategory? category = null,
        string? keyword = null,
        bool? isAvailable = null)
    {
        _category = category;
        _keyword = keyword;
        _isAvailable = isAvailable;
    }

    public override Expression<Func<Domain.Food.Food, bool>> ToExpression()
    {
        return food =>
            !food.IsDeleted &&
            (!_category.HasValue || food.Category == _category.Value) &&
            (!_isAvailable.HasValue || food.IsAvailable == _isAvailable.Value) &&
            (string.IsNullOrWhiteSpace(_keyword) ||
                food.Name.ToLower().Contains(_keyword) ||
                food.Description.ToLower().Contains(_keyword));
    }

    public override IQueryable<Domain.Food.Food> ApplyOrdering(IQueryable<Domain.Food.Food> queryable)
    {
        return queryable.OrderByDescending(f => f.CreatedAt);
    }
}