using System.Linq;
using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Period.Specifications;

public class PublishedPeriodsSpecification : Specification<Period>
{
    public PublishedPeriodsSpecification()
    {
        Include(p => p.ThumbnailMediaAsset!);
    }

    public override Expression<Func<Period, bool>> ToExpression()
    {
        return period => period.IsPublished && !period.IsDeleted;
    }

    public override IQueryable<Period> ApplyOrdering(IQueryable<Period> queryable)
    {
        return queryable.OrderBy(p => p.StartYear);
    }
}

