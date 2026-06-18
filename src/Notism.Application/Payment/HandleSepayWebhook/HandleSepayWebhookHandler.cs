using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Application.Order.CreateOrder;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Repositories;

using DomainBankingCheckout = Notism.Domain.Payment.BankingCheckout;
using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Payment.HandleSepayWebhook;

public class HandleSepayWebhookHandler : IRequestHandler<HandleSepayWebhookRequest>
{
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ISender _sender;
    private readonly IPaymentNotifier _paymentNotifier;
    private readonly ILogger<HandleSepayWebhookHandler> _logger;

    public HandleSepayWebhookHandler(
        IBankingCheckoutRepository bankingCheckoutRepository,
        IOrderRepository orderRepository,
        IReadDbContext readDbContext,
        ISender sender,
        IPaymentNotifier paymentNotifier,
        ILogger<HandleSepayWebhookHandler> logger)
    {
        _bankingCheckoutRepository = bankingCheckoutRepository;
        _orderRepository = orderRepository;
        _readDbContext = readDbContext;
        _sender = sender;
        _paymentNotifier = paymentNotifier;
        _logger = logger;
    }

    public async Task Handle(HandleSepayWebhookRequest request, CancellationToken cancellationToken)
    {
        if (string.Equals(request.TransferType, "out", StringComparison.OrdinalIgnoreCase))
        {
            await HandleOutboundRefundAsync(request, cancellationToken);
            return;
        }

        await HandleInboundCheckoutAsync(request, cancellationToken);
    }

    private async Task HandleInboundCheckoutAsync(HandleSepayWebhookRequest request, CancellationToken cancellationToken)
    {
        if (!TryExtractNFormatGuid(request.Content, out var checkoutId))
        {
            _logger.LogWarning("No valid checkoutId found in webhook content: {Content}", request.Content);
            return;
        }

        var checkout = await _readDbContext.Set<DomainBankingCheckout>(tracking: true)
            .Where(c => c.Id == checkoutId)
            .FirstOrDefaultAsync(cancellationToken);

        if (checkout is null)
        {
            _logger.LogWarning("BankingCheckout {CheckoutId} not found — ignoring webhook", checkoutId);
            return;
        }

        if (checkout.IsUsed)
        {
            _logger.LogInformation("BankingCheckout {CheckoutId} already used — ignoring webhook", checkoutId);
            await _paymentNotifier.NotifyPaymentFailureAsync(Guid.Empty, checkout.UserId, cancellationToken);
            return;
        }

        if (request.Amount != checkout.TotalAmount)
        {
            _logger.LogWarning(
                "Amount mismatch for checkout {CheckoutId}: expected {Expected}, received {Received}",
                checkoutId,
                checkout.TotalAmount,
                request.Amount);

            await _paymentNotifier.NotifyPaymentFailureAsync(Guid.Empty, checkout.UserId, cancellationToken);
            return;
        }

        var createOrderResponse = await _sender.Send(
            new CreateOrderRequest
            {
                UserId = checkout.UserId,
                PaymentMethod = "banking",
                CartItemIds = checkout.CartItemIds,
            },
            CancellationToken.None);

        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Where(o => o.Id == createOrderResponse.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            _logger.LogError(
                "Order {OrderId} not found after creation for checkout {CheckoutId}",
                createOrderResponse.OrderId,
                checkoutId);
            return;
        }

        order.MarkAsPaid(request.TransferredAt);
        await _orderRepository.SaveChangesAsync();

        checkout.MarkAsUsed();
        await _bankingCheckoutRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} marked as paid via BankingCheckout {CheckoutId}",
            order.Id,
            checkoutId);
    }

    private async Task HandleOutboundRefundAsync(HandleSepayWebhookRequest request, CancellationToken cancellationToken)
    {
        if (!TryExtractNFormatGuid(request.Content, out var refundId))
        {
            _logger.LogWarning("No valid refundId found in outbound webhook content: {Content}", request.Content);
            return;
        }

        var order = await _readDbContext.Set<DomainOrder>(tracking: true)
            .Include(o => o.Refund)
            .Where(o => o.Refund != null && o.Refund.Id == refundId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order?.Refund is null)
        {
            _logger.LogWarning("Refund {RefundId} not found — ignoring outbound webhook", refundId);
            return;
        }

        var refund = order.Refund;

        if (refund.Status != RefundStatus.Processing)
        {
            _logger.LogInformation(
                "Refund {RefundId} is {Status}, not Processing — ignoring outbound webhook",
                refundId,
                refund.Status);
            return;
        }

        if (request.Amount != refund.Amount)
        {
            _logger.LogWarning(
                "Amount mismatch for refund {RefundId}: expected {Expected}, received {Received}",
                refundId,
                refund.Amount,
                request.Amount);
            return;
        }

        order.MarkRefundPaid(request.TransactionId);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Refund {RefundId} marked as paid via SePay transaction {TransactionId}",
            refundId,
            request.TransactionId);
    }

    private static bool TryExtractNFormatGuid(string content, out Guid id)
    {
        var tokens = content.Split(
            new[] { ' ', '\t', '\n', '\r', '-' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            if (Guid.TryParseExact(token, "N", out id))
            {
                return true;
            }
        }

        id = Guid.Empty;
        return false;
    }
}