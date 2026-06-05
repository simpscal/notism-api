using MediatR;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;

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
        var payment = await _paymentRepository.FindByExpressionAsync(
            new FilterSpecification<Domain.Payment.Payment>(_ => true));

        if (payment is null)
        {
            return null;
        }

        return new GetBankAccountResponse
        {
            BankCode = payment.BankCode,
            AccountNumber = payment.AccountNumber,
            AccountHolderName = payment.AccountHolderName,
        };
    }
}
