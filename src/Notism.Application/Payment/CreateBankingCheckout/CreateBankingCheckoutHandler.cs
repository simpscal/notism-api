using MediatR;

using Notism.Domain.Payment;

namespace Notism.Application.Payment.CreateBankingCheckout;

public class CreateBankingCheckoutHandler : IRequestHandler<CreateBankingCheckoutRequest, CreateBankingCheckoutResponse>
{
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;

    public CreateBankingCheckoutHandler(IBankingCheckoutRepository bankingCheckoutRepository)
    {
        _bankingCheckoutRepository = bankingCheckoutRepository;
    }

    public async Task<CreateBankingCheckoutResponse> Handle(
        CreateBankingCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var checkout = BankingCheckout.Create(request.UserId, request.CartItemIds, request.TotalAmount);

        await _bankingCheckoutRepository.AddAsync(checkout);
        await _bankingCheckoutRepository.SaveChangesAsync();

        return new CreateBankingCheckoutResponse { CheckoutId = checkout.Id };
    }
}
