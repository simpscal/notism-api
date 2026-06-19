using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order;
using Notism.Domain.Order.Repositories;
using Notism.Domain.User.Enums;

using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.Order.CreateBankingCheckout;

public class CreateBankingCheckoutHandler : IRequestHandler<CreateBankingCheckoutRequest, CreateBankingCheckoutResponse>
{
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly IReadDbContext _readDbContext;

    public CreateBankingCheckoutHandler(
        IBankingCheckoutRepository bankingCheckoutRepository,
        IReadDbContext readDbContext)
    {
        _bankingCheckoutRepository = bankingCheckoutRepository;
        _readDbContext = readDbContext;
    }

    public async Task<CreateBankingCheckoutResponse> Handle(
        CreateBankingCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var checkout = BankingCheckout.Create(request.UserId, request.CartItemIds, request.TotalAmount);

        await _bankingCheckoutRepository.AddAsync(checkout);
        await _bankingCheckoutRepository.SaveChangesAsync();

        var storeAccount = await _readDbContext.Set<DomainBankAccount>()
            .Where(p => p.OwnerType == BankAccountOwnerType.Store)
            .FirstOrDefaultAsync(cancellationToken);

        return CreateBankingCheckoutResponse.FromDomain(checkout, storeAccount);
    }
}
