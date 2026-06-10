using Notism.Domain.Common.Repositories;
using Notism.Domain.User;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User.Repositories;

public interface IUserRepository : IRepository<User>
{
    public User Update(User user);
}