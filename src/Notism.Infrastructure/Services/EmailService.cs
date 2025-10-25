using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.User.ValueObjects;

namespace Notism.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(Email email, string resetToken)
    {
        // TODO: Implement actual email sending (e.g., using SendGrid, AWS SES, etc.)
        // For now, just log the reset token (DO NOT do this in production)
        _logger.LogInformation(
            "Password reset requested for {Email}. Reset token: {ResetToken}",
            email.Value,
            resetToken);

        await Task.Delay(100);

        _logger.LogInformation("Password reset email sent to {Email}", email.Value);
    }

    public async Task SendWelcomeEmailAsync(Email email, string username)
    {
        // TODO: Implement actual email sending
        _logger.LogInformation(
            "Welcome email sent to {Email} for user {Username}",
            email.Value,
            username);

        await Task.Delay(100);
    }
}