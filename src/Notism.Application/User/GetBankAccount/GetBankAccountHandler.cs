using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;

using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.User.GetBankAccount;

public class GetBankAccountHandler : IRequestHandler<GetBankAccountRequest, GetBankAccountResponse?>
{
    private readonly IReadDbContext _readDbContext;

    public GetBankAccountHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetBankAccountResponse?> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        var bankAccount = await _readDbContext.Set<DomainBankAccount>()
            .Where(p => p.OwnerType == request.OwnerType && p.OwnerId == request.OwnerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (bankAccount is null)
        {
            return null;
        }

        return GetBankAccountResponse.FromDomain(bankAccount);
    }
}