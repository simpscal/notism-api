using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Infrastructure.Persistence;

using NSubstitute;

namespace Notism.Application.Tests.Common;

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