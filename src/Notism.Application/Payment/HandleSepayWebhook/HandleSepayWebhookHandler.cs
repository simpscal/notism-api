using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Constants;

namespace Notism.Application.Payment.HandleSepayWebhook;

public class HandleSepayWebhookHandler : IRequestHandler<HandleSepayWebhookRequest>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<HandleSepayWebhookHandler> _logger;

    public HandleSepayWebhookHandler(
        IOrderRepository orderRepository,
        ILogger<HandleSepayWebhookHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Handle(HandleSepayWebhookRequest request, CancellationToken cancellationToken)
    {
        var rawSlug = request.Content.Split('-')[0].Trim();

        if (string.IsNullOrEmpty(rawSlug))
        {
            _logger.LogWarning("No SlugId found in webhook content");
            return;
        }

        var slugId = Slugs.OrderPrefixWithSeparator + rawSlug;

        var order = await _orderRepository.FindByExpressionAsync(
            new FilterSpecification<Domain.Order.Order>(o => o.SlugId == slugId));

        if (order is null)
        {
            _logger.LogWarning("Order {SlugId} not found for webhook", slugId);
            return;
        }

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            _logger.LogInformation(
                "Order {SlugId} already paid, ignoring duplicate webhook",
                slugId);
            return;
        }

        if (request.Amount != order.TotalAmount)
        {
            _logger.LogWarning(
                "Amount mismatch for order {SlugId}: expected {ExpectedAmount}, received {ReceivedAmount}",
                slugId,
                order.TotalAmount,
                request.Amount);
            order.MarkAsFailed();
            await _orderRepository.SaveChangesAsync();
            return;
        }

        order.MarkAsPaid(request.TransferredAt);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order {SlugId} marked as paid via SePay webhook", slugId);
    }
}
