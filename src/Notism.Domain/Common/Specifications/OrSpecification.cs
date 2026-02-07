using System.Linq.Expressions;

namespace Notism.Domain.Common.Specifications;

public class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;

        // Merge includes from both specifications
        foreach (var include in _left.Includes)
        {
            _includes.Add(include);
        }

        foreach (var include in _right.Includes)
        {
            if (!_includes.Contains(include))
            {
                _includes.Add(include);
            }
        }
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.OrElse(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}