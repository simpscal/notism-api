using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.ValueObjects;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Users;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IPasswordService _passwordService;

    public UserRepository(AppDbContext appDbContext, IPasswordService passwordService)
        : base(appDbContext)
    {
        _appDbContext = appDbContext;
        _passwordService = passwordService;
    }

    public User Update(User user)
    {
        _dbSet.Update(user);
        return user;
    }
}