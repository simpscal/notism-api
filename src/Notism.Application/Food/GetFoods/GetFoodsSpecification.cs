using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsSpecification : Specification<DomainFood>
{
    private readonly string? _category;
    private readonly string? _keyword;
    private readonly bool? _isAvailable;
    private readonly string? _sortBy;
    private readonly bool _isDescending;

    public GetFoodsSpecification(
        string? category = null,
        string? keyword = null,
        bool? isAvailable = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        _category = category;
        _keyword = keyword;
        _isAvailable = isAvailable;
        _sortBy = sortBy;
        var sortOrderEnum = sortOrder?.FromCamelCase<SortOrder>() ?? SortOrder.Asc;
        _isDescending = sortOrderEnum == SortOrder.Desc;
    }

    public override Expression<Func<DomainFood, bool>> ToExpression()
    {
        return food =>
            !food.IsDeleted &&
            (string.IsNullOrWhiteSpace(_category) || (food.Category != null && food.Category.Name == _category)) &&
            (!_isAvailable.HasValue || food.IsAvailable == _isAvailable.Value) &&
            (string.IsNullOrWhiteSpace(_keyword) ||
                food.Name.ToLower().Contains(_keyword) ||
                food.Description.ToLower().Contains(_keyword));
    }

    public override IQueryable<DomainFood> ApplyOrdering(IQueryable<DomainFood> queryable)
    {
        return _sortBy switch
        {
            "name" => _isDescending
                ? queryable.OrderByDescending(f => f.Name)
                : queryable.OrderBy(f => f.Name),
            "price" => _isDescending
                ? queryable.OrderByDescending(f => f.Price)
                : queryable.OrderBy(f => f.Price),
            "discountPrice" => _isDescending
                ? queryable.OrderByDescending(f => f.DiscountPrice)
                : queryable.OrderBy(f => f.DiscountPrice),
            _ => queryable.OrderByDescending(f => f.CreatedAt),
        };
    }
}