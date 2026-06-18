using MediatR;

using Notism.Domain.Payment.Enums;

namespace Notism.Application.Payment.SaveBankAccount;

public class SaveBankAccountRequest : IRequest
{
    public Guid OwnerId { get; set; }
    public PaymentOwnerType OwnerType { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}