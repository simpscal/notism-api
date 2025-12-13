using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Period.Specifications;

public class PublishedPeriodsSpecification : Specification<Period>
{
    public override Expression<Func<Period, bool>> ToExpression()
    {
        return period => period.IsPublished && !period.IsDeleted;
    }
}

