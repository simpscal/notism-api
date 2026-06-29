using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Infrastructure.Persistence;

using Npgsql;

using NSubstitute;

using Testcontainers.PostgreSql;

namespace Notism.Application.Tests.Common;

public sealed class PostgresReadDbContextFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public AppDbContext DbContext { get; private set; } = null!;

    public string ConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();
        await WaitForConnectionAsync(ConnectionString);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        DbContext = new AppDbContext(options, Substitute.For<IMediator>());
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (DbContext is not null)
        {
            await DbContext.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    // Some Docker hosts (e.g. colima) report the container ready before host
    // port-forwarding is wired, so the first connection can be refused. Retry until
    // the mapped port accepts connections; on hosts that forward instantly the first
    // attempt succeeds.
    private static async Task WaitForConnectionAsync(string connectionString)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return;
            }
            catch (NpgsqlException) when (attempt < 30)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}