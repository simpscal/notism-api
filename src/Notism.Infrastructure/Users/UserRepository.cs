using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Notism.Domain.User;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Users;

public class UserRepository(AppDbContext appDbContext) :
    Repository<User>(appDbContext),
    IUserRepository
{
    public async Task<User> AddAsync(User user)
    {
        var passwordHasher = new PasswordHasher<object>();
        var hashPassword = passwordHasher.HashPassword(new object(), user.Password);

        var newUser = new User { Email = user.Email, Password = hashPassword };

        await _dbSet.AddAsync(newUser);

        return user;
    }
}