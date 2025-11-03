using System.Collections.Generic;
using System.Linq.Expressions;

namespace Notism.Domain.Common.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
    IReadOnlyCollection<Expression<Func<T, object>>> Includes { get; }
}