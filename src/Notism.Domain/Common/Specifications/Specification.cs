using System.Collections.Generic;
using System.Linq.Expressions;

using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.Common.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = new();

    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public IReadOnlyCollection<Expression<Func<T, object>>> Includes => _includes.AsReadOnly();

    public Specification<T> Include(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
        return this;
    }

    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }

    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}