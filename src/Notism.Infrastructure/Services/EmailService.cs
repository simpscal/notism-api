using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Domain.User.ValueObjects;

using Resend;

namespace Notism.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IResend _resendClient;
    private readonly IConfiguration _configuration;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(
        ILogger<EmailService> logger,
        IResend resendClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _resendClient = resendClient;
        _configuration = configuration;
        _fromEmail = configuration["Email:FromEmail"] ?? throw new ArgumentNullException("Email:FromEmail configuration is missing");
        _fromName = configuration["Email:FromName"] ?? "Notism";
    }

    public async Task SendPasswordResetEmailAsync(Email email, string resetToken)
    {
        var subject = "Reset Your Password";
        var clientAppUrl = _configuration["ClientApp:Url"] ?? throw new ArgumentNullException("ClientApp:Url configuration is missing");
        var resetUrl = $"{clientAppUrl}/auth/reset-password?token={resetToken}";

        var htmlContent = LoadEmailTemplate("PasswordReset.html")
            .Replace("{{RESET_TOKEN}}", resetToken)
            .Replace("{{RESET_URL}}", resetUrl)
            .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

        var message = new EmailMessage
        {
            From = $"{_fromName} <{_fromEmail}>",
            To = [email.Value],
            Subject = subject,
            HtmlBody = htmlContent,
        };

        var response = await _resendClient.EmailSendAsync(message);

        _logger.LogInformation("Password reset email sent successfully to {Email}. MessageId: {MessageId}", email.Value, response);
    }

    public async Task SendWelcomeEmailAsync(Email email, string username)
    {
        var subject = "Welcome to Notism!";

        var htmlContent = LoadEmailTemplate("Welcome.html")
            .Replace("{{USERNAME}}", username)
            .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

        var message = new EmailMessage
        {
            From = $"{_fromName} <{_fromEmail}>",
            To = [email.Value],
            Subject = subject,
            HtmlBody = htmlContent,
        };

        var response = await _resendClient.EmailSendAsync(message);

        _logger.LogInformation("Welcome email sent successfully to {Email} for user {Username}. MessageId: {MessageId}", email.Value, username, response);
    }

    private static string LoadEmailTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Notism.Infrastructure.EmailTemplates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Email template '{templateName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}