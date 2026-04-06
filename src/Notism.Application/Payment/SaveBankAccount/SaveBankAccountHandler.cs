using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;

namespace Notism.Application.Payment.SaveBankAccount;

public class SaveBankAccountHandler : IRequestHandler<SaveBankAccountRequest>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<SaveBankAccountHandler> _logger;

    public SaveBankAccountHandler(
        IPaymentRepository paymentRepository,
        ILogger<SaveBankAccountHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task Handle(SaveBankAccountRequest request, CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Payment.Payment>(p => p.StorerId == request.StorerId);
        var existing = await _paymentRepository.FindByExpressionAsync(specification);

        if (existing is null)
        {
            var payment = Domain.Payment.Payment.Create(
                request.StorerId,
                request.BankCode,
                request.AccountNumber,
                request.AccountHolderName);

            _paymentRepository.Add(payment);
        }
        else
        {
            existing.Update(request.BankCode, request.AccountNumber, request.AccountHolderName);
        }

        await _paymentRepository.SaveChangesAsync();

        _logger.LogInformation("Saved bank account for storer {StorerId}", request.StorerId);
    }
}
