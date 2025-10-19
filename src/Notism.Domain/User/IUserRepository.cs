using Notism.Domain.Common.Interfaces;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User;

public interface IUserRepository : IRepository<User>
{
    public Task<User> AddAsync(User user);
}