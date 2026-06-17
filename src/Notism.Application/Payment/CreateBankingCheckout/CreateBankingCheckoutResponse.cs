using Notism.Domain.Payment;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Payment.CreateBankingCheckout;

public sealed record CreateBankingCheckoutResponse
{
    public Guid CheckoutId { get; set; }
    public CheckoutBankAccountResponse? BankAccount { get; set; }

    public static CreateBankingCheckoutResponse FromDomain(BankingCheckout checkout, DomainPayment? storeAccount)
    {
        return new CreateBankingCheckoutResponse
        {
            CheckoutId = checkout.Id,
            BankAccount = storeAccount == null ? null : CheckoutBankAccountResponse.FromDomain(storeAccount),
        };
    }
}