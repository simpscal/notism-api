namespace Notism.Application.Common.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string plainTextPassword);
}