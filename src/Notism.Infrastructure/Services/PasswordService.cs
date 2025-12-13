using Microsoft.AspNetCore.Identity;

using Notism.Application.Common.Interfaces;

namespace Notism.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _passwordHasher;

    public PasswordService()
    {
        _passwordHasher = new PasswordHasher<object>();
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string hashedPassword, string plainTextPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), hashedPassword, plainTextPassword);
        return result == PasswordVerificationResult.Success;
    }
}