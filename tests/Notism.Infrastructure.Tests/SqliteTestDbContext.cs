using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Tests;

/// <summary>
/// Test-only <see cref="AppDbContext"/> for the in-memory SQLite provider.
/// SQLite has no native decimal type and refuses to translate <c>SUM</c> over
/// decimal columns, so the conventions here map decimal to a double-backed
/// converter. Production runs on Npgsql where decimal SUM is fully supported and
/// keeps full precision; this shim only affects tests.
/// </summary>
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