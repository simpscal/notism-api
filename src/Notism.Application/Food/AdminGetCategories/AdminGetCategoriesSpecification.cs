using System.Linq;
using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.AdminGetCategories;

public class AdminGetCategoriesSpecification : Specification<DomainCategory>
{
    public override Expression<Func<DomainCategory, bool>> ToExpression()
    {
        return c => !c.IsDeleted;
    }

    public override IQueryable<DomainCategory> ApplyOrdering(IQueryable<DomainCategory> queryable)
    {
        return queryable.OrderBy(c => c.Name);
    }
}
