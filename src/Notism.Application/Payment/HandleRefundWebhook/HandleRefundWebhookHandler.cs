using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Shared.Configuration;
using Notism.Shared.Exceptions;

using DomainOrder = Notism.Domain.Order.Order;
using DomainRefund = Notism.Domain.Order.Refund;

namespace Notism.Application.Payment.HandleRefundWebhook;

public class HandleRefundWebhookHandler : IRequestHandler<HandleRefundWebhookRequest>
{
    private const string SuccessStatus = "success";

    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly BankingRefundSettings _settings;
    private readonly ILogger<HandleRefundWebhookHandler> _logger;

    public HandleRefundWebhookHandler(
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        IOptions<BankingRefundSettings> settings,
        ILogger<HandleRefundWebhookHandler> logger)
    {
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task Handle(HandleRefundWebhookRequest request, CancellationToken cancellationToken)
    {
        VerifySecret(request.Secret);

        var refund = await _readDbContext.Set<DomainRefund>()
            .Where(r => r.TransferReference == request.TransferReference)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"No refund found for transfer reference {request.TransferReference}");

        if (refund.Status != RefundStatus.Processing)
        {
            _logger.LogInformation(
                "Refund {RefundId} is {Status}, not Processing — ignoring webhook redelivery",
                refund.Id,
                refund.Status);
            return;
        }

        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Include(o => o.Refund)
            .Where(o => o.Id == refund.OrderId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Order {refund.OrderId} not found for refund {refund.Id}");

        if (IsSuccess(request.Status))
        {
            order.MarkRefundPaid();
            _logger.LogInformation("Refund {RefundId} marked as paid", refund.Id);
        }
        else
        {
            order.MarkRefundFailed(request.FailureReason ?? "Bank transfer failed");
            _logger.LogInformation("Refund {RefundId} marked as failed", refund.Id);
        }

        await _orderRepository.SaveChangesAsync();
    }

    private void VerifySecret(string secret)
    {
        if (string.IsNullOrEmpty(_settings.WebhookSecret) || secret != _settings.WebhookSecret)
        {
            throw new ResultFailureException("Invalid webhook signature");
        }
    }

    private static bool IsSuccess(string status)
        => string.Equals(status, SuccessStatus, StringComparison.OrdinalIgnoreCase);
}