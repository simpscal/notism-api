using Microsoft.Extensions.Options;

using Notism.Application.Common.Services;
using Notism.Shared.Configuration;

namespace Notism.Infrastructure.Services;

public class StubBankTransferService : IBankTransferService
{
    private readonly BankingRefundSettings _settings;

    public StubBankTransferService(IOptions<BankingRefundSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<BankTransferResult> InitiateTransferAsync(
        InitiateBankTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_settings.AlwaysReject)
        {
            return Task.FromResult(BankTransferResult.Reject(_settings.RejectReason));
        }

        return Task.FromResult(BankTransferResult.Accept(request.IdempotencyKey));
    }
}