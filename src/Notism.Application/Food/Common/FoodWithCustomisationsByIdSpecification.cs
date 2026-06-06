using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.Common;

/// <summary>
/// Loads a single, non-deleted food with its customisation groups and the
/// options under each group. Reused by every admin customisation handler so the
/// include chain is defined in exactly one place.
/// </summary>
public class FoodWithCustomisationsByIdSpecification : Specification<DomainFood>
{
    private readonly Guid _foodId;

    public FoodWithCustomisationsByIdSpecification(Guid foodId)
    {
        _foodId = foodId;

        Include("CustomisationGroups.Options");
    }

    public override Expression<Func<DomainFood, bool>> ToExpression()
    {
        return food => food.Id == _foodId && !food.IsDeleted;
    }
}