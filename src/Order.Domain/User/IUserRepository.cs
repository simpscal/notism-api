using Order.Domain.Common.Interfaces;

namespace Order.Domain.User;

public interface IUserRepository : IRepository<User>
{
    public Task<User> AddAsync(User user);
}