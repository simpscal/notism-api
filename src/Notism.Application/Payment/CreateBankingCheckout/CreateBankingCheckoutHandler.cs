using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Domain.Payment;
using Notism.Domain.Payment.Enums;
using Notism.Domain.Payment.Repositories;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Payment.CreateBankingCheckout;

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

        var storeAccount = await _readDbContext.Set<DomainPayment>()
            .Where(p => p.OwnerType == PaymentOwnerType.Store)
            .FirstOrDefaultAsync(cancellationToken);

        return CreateBankingCheckoutResponse.FromDomain(checkout, storeAccount);
    }
}