using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.User.Repositories;

using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.User.SaveBankAccount;

public class SaveBankAccountHandler : IRequestHandler<SaveBankAccountRequest>
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<SaveBankAccountHandler> _logger;

    public SaveBankAccountHandler(
        IBankAccountRepository bankAccountRepository,
        IReadDbContext readDbContext,
        ILogger<SaveBankAccountHandler> logger)
    {
        _bankAccountRepository = bankAccountRepository;
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task Handle(SaveBankAccountRequest request, CancellationToken cancellationToken)
    {
        var existing = await _readDbContext.Set<DomainBankAccount>(tracking: true)
            .Where(p => p.OwnerType == request.OwnerType && p.OwnerId == request.OwnerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            var bankAccount = DomainBankAccount.Create(
                request.OwnerType,
                request.OwnerId,
                request.BankCode,
                request.AccountNumber,
                request.AccountHolderName);

            _bankAccountRepository.Add(bankAccount);
        }
        else
        {
            existing.Update(request.BankCode, request.AccountNumber, request.AccountHolderName);
        }

        await _bankAccountRepository.SaveChangesAsync();

        _logger.LogInformation("Saved {OwnerType} bank account for owner {OwnerId}", request.OwnerType, request.OwnerId);
    }
}