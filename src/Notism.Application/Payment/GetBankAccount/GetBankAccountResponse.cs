namespace Notism.Application.Payment.GetBankAccount;

public sealed record GetBankAccountResponse
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;

    public static GetBankAccountResponse FromDomain(Domain.Payment.Payment payment)
    {
        return new GetBankAccountResponse
        {
            BankCode = payment.BankCode,
            AccountNumber = payment.AccountNumber,
            AccountHolderName = payment.AccountHolderName,
        };
    }
}