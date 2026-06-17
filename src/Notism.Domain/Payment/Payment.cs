using Notism.Domain.Common;
using Notism.Domain.Payment.Enums;

namespace Notism.Domain.Payment;

public class Payment : AggregateRoot
{
    public PaymentOwnerType OwnerType { get; private set; }
    public Guid StorerId { get; private set; }
    public string BankCode { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public string AccountHolderName { get; private set; } = string.Empty;

    private Payment(
        PaymentOwnerType ownerType,
        Guid storerId,
        string bankCode,
        string accountNumber,
        string accountHolderName)
    {
        OwnerType = ownerType;
        StorerId = storerId;
        BankCode = bankCode;
        AccountNumber = accountNumber;
        AccountHolderName = accountHolderName;
    }

    public static Payment Create(
        PaymentOwnerType ownerType,
        Guid storerId,
        string bankCode,
        string accountNumber,
        string accountHolderName)
    {
        return new Payment(ownerType, storerId, bankCode, accountNumber, accountHolderName);
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