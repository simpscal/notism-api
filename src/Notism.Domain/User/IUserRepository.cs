using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.User;

public interface IUserRepository : IRepository<User>
{
    public Task<User> AddAsync(User user);
}