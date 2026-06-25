using MediatR;

using Notism.Domain.User.Enums;

namespace Notism.Application.User.GetBankAccount;

public class GetBankAccountRequest : IRequest<GetBankAccountResponse?>
{
    public Guid OwnerId { get; set; }
    public BankAccountOwnerType OwnerType { get; set; }
}