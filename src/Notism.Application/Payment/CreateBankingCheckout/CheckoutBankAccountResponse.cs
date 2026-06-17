using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Payment.CreateBankingCheckout;

public sealed record CheckoutBankAccountResponse
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;

    public static CheckoutBankAccountResponse FromDomain(DomainPayment payment)
    {
        return new CheckoutBankAccountResponse
        {
            BankCode = payment.BankCode,
            AccountNumber = payment.AccountNumber,
            AccountHolderName = payment.AccountHolderName,
        };
    }
}