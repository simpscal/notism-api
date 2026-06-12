using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Infrastructure.Persistence;
using Notism.Infrastructure.Repositories;

using NSubstitute;

namespace Notism.Application.Tests.Common;

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