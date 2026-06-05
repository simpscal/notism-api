using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Payment;
using Notism.Shared.Exceptions;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountHandler : IRequestHandler<GetBankAccountRequest, GetBankAccountResponse?>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly ILogger<GetBankAccountHandler> _logger;

    public GetBankAccountHandler(
        IPaymentRepository paymentRepository,
        IBankingCheckoutRepository bankingCheckoutRepository,
        ILogger<GetBankAccountHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _bankingCheckoutRepository = bankingCheckoutRepository;
        _logger = logger;
    }

    public async Task<GetBankAccountResponse?> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        Domain.Payment.Payment? payment;

        if (request.CheckoutId.HasValue)
        {
            // Consumer path: verify the checkout belongs to the requesting user,
            // then return the storer's bank account for QR display.
            var checkout = await _bankingCheckoutRepository.FindByExpressionAsync(
                new FilterSpecification<BankingCheckout>(c => c.Id == request.CheckoutId.Value));

            if (checkout is null)
            {
                throw new NotFoundException($"BankingCheckout {request.CheckoutId.Value} not found.");
            }

            if (checkout.UserId != request.UserId)
            {
                throw new ForbiddenException("You do not have access to this checkout.");
            }

            // There is one storer — retrieve the single payment/bank-account record.
            payment = await _paymentRepository.FindByExpressionAsync(
                new FilterSpecification<Domain.Payment.Payment>(_ => true));

            _logger.LogInformation(
                "Consumer {UserId} retrieved bank account via checkout {CheckoutId}",
                request.UserId,
                request.CheckoutId.Value);
        }
        else
        {
            // Admin path: retrieve the storer's own bank account record.
            payment = await _paymentRepository.FindByExpressionAsync(
                new FilterSpecification<Domain.Payment.Payment>(p => p.StorerId == request.StorerId));

            _logger.LogInformation("Retrieved bank account for storer {StorerId}", request.StorerId);
        }

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
