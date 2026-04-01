using MediatR;

namespace Notism.Application.Payment.SaveBankAccount;

public class SaveBankAccountRequest : IRequest
{
    public Guid StorerId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}
