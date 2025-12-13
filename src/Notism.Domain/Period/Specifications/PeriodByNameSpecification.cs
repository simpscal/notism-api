using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Period.Specifications;

public class PeriodByNameSpecification : Specification<Period>
{
    private readonly string _name;

    public PeriodByNameSpecification(string name)
    {
        _name = name;
    }

    public override Expression<Func<Period, bool>> ToExpression()
    {
        return period => period.Name == _name && !period.IsDeleted;
    }
}

