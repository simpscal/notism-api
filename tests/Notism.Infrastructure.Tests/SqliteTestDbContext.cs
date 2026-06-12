using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Tests;

public sealed class SqliteTestDbContext : AppDbContext
{
    public SqliteTestDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
        : base(options, mediator)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<decimal>()
            .HaveConversion<double>();
    }
}