using System.Linq.Expressions;

namespace Notism.Domain.Common.Repositories;

/// <summary>
/// Declarative include set for graph loads composed over the read port. A handler
/// describes the navigation graph it needs (typed lambdas and/or string navigation
/// paths); the read port's <c>BuildGraphQuery</c> applies them when building the
/// queryable, in tracking or no-tracking mode.
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
