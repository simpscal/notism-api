namespace Notism.Application.Common.Services;

public interface IBankTransferService
{
    Task<BankTransferResult> InitiateTransferAsync(
        InitiateBankTransferRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class InitiateBankTransferRequest
{
    public required Guid RefundId { get; init; }
    public required decimal Amount { get; init; }

    public string IdempotencyKey => RefundId.ToString("N");
}

public sealed class BankTransferResult
{
    private BankTransferResult(bool accepted, string? providerRef, string? reason)
    {
        Accepted = accepted;
        ProviderRef = providerRef;
        Reason = reason;
    }

    public bool Accepted { get; }
    public string? ProviderRef { get; }
    public string? Reason { get; }

    public static BankTransferResult Accept(string providerRef)
        => new(accepted: true, providerRef: providerRef, reason: null);

    public static BankTransferResult Reject(string reason)
        => new(accepted: false, providerRef: null, reason: reason);
}