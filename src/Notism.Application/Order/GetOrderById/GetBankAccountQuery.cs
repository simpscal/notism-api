using Notism.Application.Common.Persistence;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Order.GetOrderById;

/// <summary>
/// Self-contained read: resolves the store's configured bank account (single-row lookup)
/// so an unpaid banking order can surface its payment QR. Owned by
/// <see cref="GetOrderByIdHandler"/>; duplicated inline here rather than shared with any
/// other handler.
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
