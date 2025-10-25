using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Interfaces;

namespace Notism.Infrastructure.Common;

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
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var result = await operation();
            await transaction.CommitAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed, rolling back");

            try
            {
                await transaction.RollbackAsync();
                _logger.LogDebug("Transaction rolled back successfully");
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback transaction");
            }

            throw;
        }
    }
}