using Notism.Application.Common.Persistence;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Payment.GetBankAccount;

/// <summary>
/// Self-contained read: resolves the store's configured bank account (single-row
/// lookup). Owned by <see cref="GetBankAccountHandler"/> and shared with no other
/// handler.
/// </summary>
public sealed class GetBankAccountQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetBankAccountQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<DomainPayment?> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<DomainPayment>();

        return _readDbContext.FirstOrDefaultAsync(query, cancellationToken);
    }
}
