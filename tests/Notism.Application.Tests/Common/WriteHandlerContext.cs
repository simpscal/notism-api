using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Infrastructure.Persistence;
using Notism.Infrastructure.Repositories;

using NSubstitute;

namespace Notism.Application.Tests.Common;

/// <summary>
/// EF Core InMemory-backed <see cref="AppDbContext"/> for exercising read-modify-write
/// handlers end to end. The same context instance backs both the <c>IReadDbContext</c> the
/// handler reads through (tracked loads) and the <c>Repository&lt;T&gt;</c> it saves through,
/// so a tracked-load-then-SaveChanges path persists exactly as it does in production.
/// </summary>
public sealed class WriteHandlerContext : IDisposable
{
    public WriteHandlerContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"write-tests-{Guid.NewGuid():N}")
            .Options;

        DbContext = new AppDbContext(options, Substitute.For<IMediator>());
    }

    public AppDbContext DbContext { get; }

    public Repository<T> RepositoryFor<T>()
        where T : class
        => new(DbContext);

    /// <summary>
    /// Persists the given entities and clears the change tracker, so a subsequent handler
    /// load starts from a clean tracked graph.
    /// </summary>
    public async Task SeedAsync(params object[] entities)
    {
        foreach (var entity in entities)
        {
            DbContext.Add(entity);
        }

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();
    }

    public void Dispose()
        => DbContext.Dispose();
}
