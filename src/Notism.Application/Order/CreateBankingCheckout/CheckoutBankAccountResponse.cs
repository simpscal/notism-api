using DomainBankAccount = Notism.Domain.User.BankAccount;

namespace Notism.Application.Order.CreateBankingCheckout;

public sealed record CheckoutBankAccountResponse
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;

    public static CheckoutBankAccountResponse FromDomain(DomainBankAccount bankAccount)
    {
        return new CheckoutBankAccountResponse
        {
            BankCode = bankAccount.BankCode,
            AccountNumber = bankAccount.AccountNumber,
            AccountHolderName = bankAccount.AccountHolderName,
        };
    }
}