using Notism.Application.Common.Persistence;

using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.GetAvailableFoodCount;

/// <summary>
/// Self-contained read: counts the available, non-deleted foods, optionally scoped to a
/// category. Owned by <see cref="GetAvailableFoodCountHandler"/>; the not-deleted /
/// available predicate is duplicated inline here rather than shared with any other
/// handler.
/// <para>This read was added without touching any repository or persistence type — it
/// composes its predicate over <see cref="IReadDbContext.Set{T}"/> and materialises a
/// count through the port. New reads need no new repository methods.</para>
/// </summary>
public sealed class GetAvailableFoodCountQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetAvailableFoodCountQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<int> ExecuteAsync(string? category, CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<DomainFood>()
            .Where(f => !f.IsDeleted
                && f.IsAvailable
                && (string.IsNullOrWhiteSpace(category) || (f.Category != null && f.Category.Name == category)));

        return _readDbContext.CountAsync(query, cancellationToken);
    }
}
