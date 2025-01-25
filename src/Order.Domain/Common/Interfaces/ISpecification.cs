using System.Linq.Expressions;

namespace Order.Domain.Common.Interfaces;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}