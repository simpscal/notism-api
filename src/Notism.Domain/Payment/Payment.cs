using Notism.Domain.Common;

namespace Notism.Domain.Payment;

public class Payment : AggregateRoot
{
    public Guid StorerId { get; private set; }
    public string BankCode { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public string AccountHolderName { get; private set; } = string.Empty;

    private Payment(Guid storerId, string bankCode, string accountNumber, string accountHolderName)
    {
        StorerId = storerId;
        BankCode = bankCode;
        AccountNumber = accountNumber;
        AccountHolderName = accountHolderName;
    }

    public static Payment Create(Guid storerId, string bankCode, string accountNumber, string accountHolderName)
    {
        return new Payment(storerId, bankCode, accountNumber, accountHolderName);
    }

    public void Update(string bankCode, string accountNumber, string accountHolderName)
    {
        BankCode = bankCode;
        AccountNumber = accountNumber;
        AccountHolderName = accountHolderName;
        UpdatedAt = DateTime.UtcNow;
    }

    private Payment()
    {
    }
}
