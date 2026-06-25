using System.Globalization;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notism.Application.Common.Services;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Configuration;

namespace Notism.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _emailSettings;
    private readonly ClientAppSettings _clientAppSettings;

    public EmailService(
        ILogger<EmailService> logger,
        HttpClient httpClient,
        IOptions<EmailSettings> emailSettings,
        IOptions<ClientAppSettings> clientAppSettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _emailSettings = emailSettings.Value;
        _clientAppSettings = clientAppSettings.Value;
    }

    public async Task SendPasswordResetEmailAsync(Email email, string resetToken)
    {
        var subject = "Reset Your Password";
        var resetUrl = $"{_clientAppSettings.Url}/auth/reset-password?token={resetToken}";

        var htmlContent = LoadEmailTemplate("PasswordReset.html")
            .Replace("{{RESET_TOKEN}}", resetToken)
            .Replace("{{RESET_URL}}", resetUrl)
            .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

        await SendEmailAsync(email.Value, subject, htmlContent);

        _logger.LogInformation("Password reset email sent successfully to {Email}", email.Value);
    }

    public async Task SendWelcomeEmailAsync(Email email, string username)
    {
        var subject = "Welcome to Notism!";

        var htmlContent = LoadEmailTemplate("Welcome.html")
            .Replace("{{USERNAME}}", username)
            .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

        await SendEmailAsync(email.Value, subject, htmlContent);

        _logger.LogInformation("Welcome email sent successfully to {Email} for user {Username}", email.Value, username);
    }

    public async Task SendRefundPaidEmailAsync(
        Email email,
        string? customerFirstName,
        string orderRef,
        decimal amount,
        string transferReference,
        DateTime sentDate,
        string orderSlugId)
    {
        var subject = $"Your refund for order {orderRef} has been sent";
        var orderUrl = $"{_clientAppSettings.Url}/orders/{orderSlugId}";
        var greetingName = string.IsNullOrWhiteSpace(customerFirstName) ? "there" : customerFirstName;

        var htmlContent = LoadEmailTemplate("RefundPaid.html")
            .Replace("{{CUSTOMER_NAME}}", greetingName)
            .Replace("{{ORDER_REF}}", orderRef)
            .Replace("{{AMOUNT}}", FormatVnd(amount))
            .Replace("{{SENT_DATE}}", FormatDate(sentDate))
            .Replace("{{TRANSFER_REFERENCE}}", transferReference)
            .Replace("{{ORDER_URL}}", orderUrl)
            .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

        await SendEmailAsync(email.Value, subject, htmlContent);

        _logger.LogInformation("Refund-paid email sent successfully to {Email} for order {OrderRef}", email.Value, orderRef);
    }

    public async Task SendNewOrderEmailAsync(
        string? opsRecipient,
        string orderNumber,
        DateTime placedAt,
        decimal total)
    {
        if (string.IsNullOrWhiteSpace(opsRecipient))
        {
            _logger.LogWarning(
                "Ops recipient is not configured; skipping new-order email for order {OrderNumber}", orderNumber);
            return;
        }

        var subject = $"New order {orderNumber} placed";

        var htmlContent = LoadEmailTemplate("OrderPlaced.html")
            .Replace("{{ORDER_NUMBER}}", orderNumber)
            .Replace("{{PLACED_AT}}", FormatDateTime(placedAt))
            .Replace("{{TOTAL}}", FormatVnd(total))
            .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

        await SendEmailAsync(opsRecipient, subject, htmlContent);

        _logger.LogInformation("New-order email sent successfully to ops for order {OrderNumber}", orderNumber);
    }

    private static string FormatVnd(decimal amount)
        => $"{amount.ToString("#,##0", CultureInfo.InvariantCulture)} ₫";

    private static string FormatDate(DateTime date)
        => date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    private static string FormatDateTime(DateTime date)
        => date.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

    private async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var payload = new MailerSendRequest
        {
            From = new MailerSendAddress { Email = _emailSettings.FromEmail, Name = _emailSettings.FromName },
            To = [new MailerSendAddress { Email = toEmail }],
            Subject = subject,
            Html = htmlContent,
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.mailersend.com/v1/email", payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("MailerSend API returned {StatusCode}: {Error}", response.StatusCode, errorBody);
            throw new HttpRequestException($"MailerSend API error: {response.StatusCode}");
        }
    }

    private static string LoadEmailTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Notism.Infrastructure.EmailTemplates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Email template '{templateName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private sealed class MailerSendRequest
    {
        [JsonPropertyName("from")]
        public required MailerSendAddress From { get; init; }

        [JsonPropertyName("to")]
        public required MailerSendAddress[] To { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("html")]
        public required string Html { get; init; }
    }

    private sealed class MailerSendAddress
    {
        [JsonPropertyName("email")]
        public required string Email { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}