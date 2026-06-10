using MediatR;

using Notism.Application.Payment.Common;
using Notism.Domain.Payment;
using Notism.Domain.Payment.Repositories;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountHandler : IRequestHandler<GetBankAccountRequest, GetBankAccountResponse?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetBankAccountHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<GetBankAccountResponse?> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.FindByExpressionAsync(new BankAccountSpecification());

        if (payment is null)
        {
            return null;
        }

        return GetBankAccountResponse.FromDomain(payment);
    }
}