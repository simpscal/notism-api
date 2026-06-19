using Notism.Domain.Common;
using Notism.Domain.User.Enums;

namespace Notism.Domain.User;

public class BankAccount : AggregateRoot
{
    public BankAccountOwnerType OwnerType { get; private set; }
    public Guid OwnerId { get; private set; }
    public string BankCode { get; private set; } = string.Empty;
    public string AccountNumber { get; private set; } = string.Empty;
    public string AccountHolderName { get; private set; } = string.Empty;

    private BankAccount(
        BankAccountOwnerType ownerType,
        Guid ownerId,
        string bankCode,
        string accountNumber,
        string accountHolderName)
    {
        OwnerType = ownerType;
        OwnerId = ownerId;
        BankCode = bankCode;
        AccountNumber = accountNumber;
        AccountHolderName = accountHolderName;
    }

    public static BankAccount Create(
        BankAccountOwnerType ownerType,
        Guid ownerId,
        string bankCode,
        string accountNumber,
        string accountHolderName)
    {
        return new BankAccount(ownerType, ownerId, bankCode, accountNumber, accountHolderName);
    }

    public void Update(string bankCode, string accountNumber, string accountHolderName)
    {
        BankCode = bankCode;
        AccountNumber = accountNumber;
        AccountHolderName = accountHolderName;
        UpdatedAt = DateTime.UtcNow;
    }

    private BankAccount()
    {
    }
}
