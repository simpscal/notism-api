using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.CreateOrder;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Payment;

namespace Notism.Application.Payment.HandleSepayWebhook;

public class HandleSepayWebhookHandler : IRequestHandler<HandleSepayWebhookRequest>
{
    private readonly IBankingCheckoutRepository _bankingCheckoutRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISender _sender;
    private readonly INotificationService _notificationService;
    private readonly ILogger<HandleSepayWebhookHandler> _logger;

    public HandleSepayWebhookHandler(
        IBankingCheckoutRepository bankingCheckoutRepository,
        IOrderRepository orderRepository,
        ISender sender,
        INotificationService notificationService,
        ILogger<HandleSepayWebhookHandler> logger)
    {
        _bankingCheckoutRepository = bankingCheckoutRepository;
        _orderRepository = orderRepository;
        _sender = sender;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(HandleSepayWebhookRequest request, CancellationToken cancellationToken)
    {
        var trimmed = request.Content.Trim();

        if (trimmed.Length < 32 || !Guid.TryParseExact(trimmed[..32], "N", out var checkoutId))
        {
            _logger.LogWarning("No valid checkoutId found in webhook content: {Content}", request.Content);
            return;
        }

        var checkout = await _bankingCheckoutRepository.FindByExpressionAsync(
            new FilterSpecification<BankingCheckout>(c => c.Id == checkoutId));

        if (checkout is null || checkout.IsUsed)
        {
            _logger.LogInformation(
                "BankingCheckout {CheckoutId} not found or already used — ignoring webhook",
                checkoutId);
            return;
        }

        if (request.Amount != checkout.TotalAmount)
        {
            _logger.LogWarning(
                "Amount mismatch for checkout {CheckoutId}: expected {Expected}, received {Received}",
                checkoutId,
                checkout.TotalAmount,
                request.Amount);

            await _notificationService.NotifyPaymentFailureAsync(Guid.Empty, checkout.UserId, cancellationToken);
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

        var order = await _orderRepository.FindByExpressionAsync(
            new FilterSpecification<Domain.Order.Order>(o => o.Id == createOrderResponse.OrderId));

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
}
