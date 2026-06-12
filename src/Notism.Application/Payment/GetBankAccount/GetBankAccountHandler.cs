using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountHandler : IRequestHandler<GetBankAccountRequest, GetBankAccountResponse?>
{
    private readonly IReadDbContext _readDbContext;

    public GetBankAccountHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetBankAccountResponse?> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        var payment = await _readDbContext.Set<DomainPayment>().FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            return null;
        }

        return GetBankAccountResponse.FromDomain(payment);
    }
}