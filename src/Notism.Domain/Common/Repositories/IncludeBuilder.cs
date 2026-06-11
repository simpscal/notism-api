using System.Linq.Expressions;

namespace Notism.Domain.Common.Repositories;

/// <summary>
/// Declarative include set for the tracked write-path loads on
/// <see cref="IRepository{T}"/>. A write handler describes the navigation graph it
/// needs to load and mutate (typed lambdas and/or string navigation paths); the
/// repository applies them when materialising the tracked aggregate.
/// <para>This is the write boundary's only graph concern. Read projections live in
/// the Application query objects over <c>IReadDbContext</c> and never go through
/// here.</para>
/// </summary>
public sealed class IncludeBuilder<T>
    where T : class
{
    private readonly List<Expression<Func<T, object>>> _expressionIncludes = new();
    private readonly List<string> _stringIncludes = new();

    public IReadOnlyList<Expression<Func<T, object>>> ExpressionIncludes => _expressionIncludes;

    public IReadOnlyList<string> StringIncludes => _stringIncludes;

    public IncludeBuilder<T> Include(Expression<Func<T, object>> include)
    {
        _expressionIncludes.Add(include);
        return this;
    }

    public IncludeBuilder<T> Include(string includePath)
    {
        _stringIncludes.Add(includePath);
        return this;
    }
}
