using System.Text.RegularExpressions;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Payment.Enums;

namespace Notism.Application.Payment.HandleSepayWebhook;

public class HandleSepayWebhookHandler : IRequestHandler<HandleSepayWebhookRequest>
{
    private static readonly Regex SlugIdPattern = new(@"ORD-?[A-Z0-9]+", RegexOptions.Compiled);

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
        var match = SlugIdPattern.Match(request.Description);
        if (!match.Success)
        {
            _logger.LogWarning("No SlugId found in webhook description");
            return;
        }

        var slugId = match.Value;
        if (slugId.Length > 3 && slugId[3] != '-')
        {
            slugId = "ORD-" + slugId[3..];
        }

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
            return;
        }

        order.MarkAsPaid(request.TransferredAt);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Order {SlugId} marked as paid via SePay webhook", slugId);
    }
}
