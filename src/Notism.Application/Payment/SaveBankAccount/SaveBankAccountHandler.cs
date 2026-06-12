using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Domain.Payment.Repositories;

using DomainPayment = Notism.Domain.Payment.Payment;

namespace Notism.Application.Payment.SaveBankAccount;

public class SaveBankAccountHandler : IRequestHandler<SaveBankAccountRequest>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<SaveBankAccountHandler> _logger;

    public SaveBankAccountHandler(
        IPaymentRepository paymentRepository,
        IReadDbContext readDbContext,
        ILogger<SaveBankAccountHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task Handle(SaveBankAccountRequest request, CancellationToken cancellationToken)
    {
        var existing = await _readDbContext.Set<DomainPayment>(tracking: true)
            .Where(p => p.StorerId == request.StorerId)
            .FirstOrDefaultAsync(cancellationToken);

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