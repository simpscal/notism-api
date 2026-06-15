using FluentAssertions;

using Microsoft.Extensions.Options;

using Notism.Application.Common.Services;
using Notism.Infrastructure.Services;
using Notism.Shared.Configuration;

namespace Notism.Infrastructure.Tests.Services;

public class StubBankTransferServiceTests
{
    [Fact]
    public async Task InitiateTransferAsync_WhenNotConfiguredToReject_AcceptsWithRefundIdAsProviderRef()
    {
        var refundId = Guid.NewGuid();
        var service = CreateService(new BankingRefundSettings());

        var result = await service.InitiateTransferAsync(
            new InitiateBankTransferRequest { RefundId = refundId, Amount = 100m });

        result.Accepted.Should().BeTrue();
        result.ProviderRef.Should().Be(refundId.ToString("N"));
        result.Reason.Should().BeNull();
    }

    [Fact]
    public async Task InitiateTransferAsync_WhenConfiguredToReject_RejectsWithReason()
    {
        var service = CreateService(new BankingRefundSettings
        {
            AlwaysReject = true,
            RejectReason = "Provider down",
        });

        var result = await service.InitiateTransferAsync(
            new InitiateBankTransferRequest { RefundId = Guid.NewGuid(), Amount = 100m });

        result.Accepted.Should().BeFalse();
        result.Reason.Should().Be("Provider down");
        result.ProviderRef.Should().BeNull();
    }

    [Fact]
    public void IdempotencyKey_IsRefundId()
    {
        var refundId = Guid.NewGuid();

        var request = new InitiateBankTransferRequest { RefundId = refundId, Amount = 100m };

        request.IdempotencyKey.Should().Be(refundId.ToString("N"));
    }

    private static StubBankTransferService CreateService(BankingRefundSettings settings)
        => new(Options.Create(settings));
}