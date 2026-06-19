using MediatR;

using Notism.Domain.User.Enums;

namespace Notism.Application.User.SaveBankAccount;

public class SaveBankAccountRequest : IRequest
{
    public Guid OwnerId { get; set; }
    public BankAccountOwnerType OwnerType { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}
