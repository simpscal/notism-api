namespace Notism.Application.User.GetBankAccount;

public sealed record GetBankAccountResponse
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;

    public static GetBankAccountResponse FromDomain(Domain.User.BankAccount bankAccount)
    {
        return new GetBankAccountResponse
        {
            BankCode = bankAccount.BankCode,
            AccountNumber = bankAccount.AccountNumber,
            AccountHolderName = bankAccount.AccountHolderName,
        };
    }
}