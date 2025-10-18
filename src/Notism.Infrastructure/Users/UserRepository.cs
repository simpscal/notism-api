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

    public Task<User> AddAsync(User user)
    {
        var hashedPassword = _passwordService.HashPassword(user.Password);
        var userForPersistence = user.WithHashedPassword(hashedPassword);

        _appDbContext.Users.Add(userForPersistence);

        return Task.FromResult(userForPersistence);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.Value == email);
    }

    public async Task<User?> GetByEmailAsync(Email email)
    {
        return await GetByEmailAsync(email.Value);
    }

    public async Task<bool> EmailExistsAsync(Email email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email.Value.ToLower() == email.Value.ToLower());
    }
}