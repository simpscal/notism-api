using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Payment.Repositories;

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
        var existing = await _paymentRepository.GetForUpdateAsync(p => p.StorerId == request.StorerId);

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