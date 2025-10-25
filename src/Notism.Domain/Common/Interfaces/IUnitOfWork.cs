namespace Notism.Domain.Common.Interfaces;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<Task> operation);
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
}