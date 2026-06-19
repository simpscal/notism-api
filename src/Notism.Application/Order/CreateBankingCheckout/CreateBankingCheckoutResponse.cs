using Notism.Domain.Order;

using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.Order.CreateBankingCheckout;

public sealed record CreateBankingCheckoutResponse
{
    public Guid CheckoutId { get; set; }
    public CheckoutBankAccountResponse? BankAccount { get; set; }

    public static CreateBankingCheckoutResponse FromDomain(BankingCheckout checkout, DomainBankAccount? storeAccount)
    {
        return new CreateBankingCheckoutResponse
        {
            CheckoutId = checkout.Id,
            BankAccount = storeAccount == null ? null : CheckoutBankAccountResponse.FromDomain(storeAccount),
        };
    }
}
