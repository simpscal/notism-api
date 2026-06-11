using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Infrastructure.Persistence;

using NSubstitute;

namespace Notism.Application.Tests.Common;

/// <summary>
/// Builds an EF Core InMemory-backed <see cref="AppDbContext"/> for exercising the
/// per-handler read query objects. The query objects compose their LINQ over
/// <see cref="IReadDbContext.Set{T}"/> and materialise through the port's async
/// operators; the InMemory provider executes that composition, letting the unit tests
/// assert filter/order/paging/projection shape without a real database.
/// <para>Each context gets a unique database name so tests are isolated. The mediator is
/// a no-op substitute since reads never dispatch domain events.</para>
/// </summary>
public static class ReadDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"read-tests-{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options, Substitute.For<IMediator>());
    }
}
