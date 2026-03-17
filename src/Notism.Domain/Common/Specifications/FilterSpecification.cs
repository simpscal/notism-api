using System.Linq.Expressions;

namespace Notism.Domain.Common.Specifications;

/// <summary>
/// Generic filter specification for common filtering patterns.
/// Reduces the need to create individual specification classes for simple filters.
/// </summary>
public class FilterSpecification<T> : Specification<T>
{
    private readonly Expression<Func<T, bool>> _filterExpression;

    public FilterSpecification(Expression<Func<T, bool>> filterExpression)
    {
        _filterExpression = filterExpression;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        return _filterExpression;
    }
}