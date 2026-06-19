namespace Notism.Application.Order.Common;

public sealed record PaymentQrResponse
{
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string OrderReference { get; set; } = string.Empty;

    public static PaymentQrResponse FromDomain(Domain.User.BankAccount bankAccount, decimal amount, string orderReference)
    {
        return new PaymentQrResponse
        {
            BankCode = bankAccount.BankCode,
            AccountNumber = bankAccount.AccountNumber,
            AccountHolderName = bankAccount.AccountHolderName,
            Amount = amount,
            OrderReference = orderReference,
        };
    }
}