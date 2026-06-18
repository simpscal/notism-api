using MediatR;

using Notism.Domain.Payment.Enums;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountRequest : IRequest<GetBankAccountResponse?>
{
    public Guid OwnerId { get; set; }
    public PaymentOwnerType OwnerType { get; set; }
}