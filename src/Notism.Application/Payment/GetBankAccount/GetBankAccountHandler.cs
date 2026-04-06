using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountHandler : IRequestHandler<GetBankAccountRequest, GetBankAccountResponse?>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetBankAccountHandler> _logger;

    public GetBankAccountHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetBankAccountHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<GetBankAccountResponse?> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Payment.Payment>(p => p.StorerId == request.StorerId);
        var payment = await _paymentRepository.FindByExpressionAsync(specification);

        if (payment is null)
        {
            return null;
        }

        _logger.LogInformation("Retrieved bank account for storer {StorerId}", request.StorerId);

        return new GetBankAccountResponse
        {
            BankCode = payment.BankCode,
            AccountNumber = payment.AccountNumber,
            AccountHolderName = payment.AccountHolderName,
        };
    }
}
