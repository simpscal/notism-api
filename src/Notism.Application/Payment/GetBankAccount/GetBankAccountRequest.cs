using MediatR;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountRequest : IRequest<GetBankAccountResponse?> { }