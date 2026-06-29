using System.Net;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notism.Infrastructure.Services;
using Notism.Shared.Configuration;

using NSubstitute;

namespace Notism.Infrastructure.Tests;

public class EmailServiceTests
{
    private const string OpsRecipient = "ops@notism.test";

    private readonly ILogger<EmailService> _logger = Substitute.For<ILogger<EmailService>>();

    [Fact]
    public async Task SendNewOrderEmailAsync_WhenOpsRecipientProvided_PostsOrderDetailsToMailerSend()
    {
        var handler = new RecordingHandler(HttpStatusCode.Accepted);
        var service = CreateService(handler);

        await service.SendNewOrderEmailAsync(
            OpsRecipient,
            "ORD-1001",
            new DateTime(2026, 6, 25, 14, 30, 0, DateTimeKind.Utc),
            250_000m);

        handler.RequestCount.Should().Be(1);
        handler.LastRequestBody.Should().Contain(OpsRecipient);
        handler.LastRequestBody.Should().Contain("ORD-1001");
        handler.LastRequestBody.Should().Contain("25 Jun 2026 14:30");
        handler.LastRequestBody.Should().Contain("250,000");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendNewOrderEmailAsync_WhenOpsRecipientMissing_LogsWarningAndDoesNotSend(string? opsRecipient)
    {
        var handler = new RecordingHandler(HttpStatusCode.Accepted);
        var service = CreateService(handler);

        var act = async () => await service.SendNewOrderEmailAsync(
            opsRecipient,
            "ORD-1001",
            DateTime.UtcNow,
            250_000m);

        await act.Should().NotThrowAsync();
        handler.RequestCount.Should().Be(0);
        _logger.ReceivedWithAnyArgs(1).Log(
            LogLevel.Warning, default, default!, default, default!);
    }

    [Fact]
    public async Task SendNewOrderEmailAsync_WhenMailerSendReturnsNon2xx_ThrowsToCaller()
    {
        var handler = new RecordingHandler(HttpStatusCode.BadRequest);
        var service = CreateService(handler);

        var act = async () => await service.SendNewOrderEmailAsync(
            OpsRecipient,
            "ORD-1001",
            DateTime.UtcNow,
            250_000m);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    private EmailService CreateService(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var emailSettings = Options.Create(new EmailSettings
        {
            FromEmail = "noreply@notism.test",
            FromName = "Notism",
        });
        var clientAppSettings = Options.Create(new ClientAppSettings { Url = "http://localhost" });

        return new EmailService(_logger, httpClient, emailSettings, clientAppSettings);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public RecordingHandler(HttpStatusCode statusCode) => _statusCode = statusCode;

        public int RequestCount { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent("{}"),
            };
        }
    }
}