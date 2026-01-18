using Notism.Domain.User.ValueObjects;

namespace Notism.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(Email email, string resetToken);
    Task SendWelcomeEmailAsync(Email email, string username);
}