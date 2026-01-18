namespace Notism.Application.Common.Interfaces;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string plainTextPassword);
}