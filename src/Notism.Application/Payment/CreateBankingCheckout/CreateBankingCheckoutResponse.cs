using Notism.Domain.Payment;

namespace Notism.Application.Payment.CreateBankingCheckout;

public sealed record CreateBankingCheckoutResponse
{
    public Guid CheckoutId { get; set; }

    public static CreateBankingCheckoutResponse FromDomain(BankingCheckout checkout)
    {
        return new CreateBankingCheckoutResponse
        {
            CheckoutId = checkout.Id,
        };
    }
}