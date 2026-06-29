using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Persistence;

namespace Notism.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await operation();
            return true;
        });
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        await _dbContext.BeginTransactionAsync();

        try
        {
            var result = await operation();
            await _dbContext.SaveChangesAsync();
            await _dbContext.CommitTransactionAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed, rolling back");
            await _dbContext.RollbackTransactionAsync();

            throw;
        }
    }
}