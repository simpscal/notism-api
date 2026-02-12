using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Shared.Exceptions;

namespace Notism.Application.Order.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderRequest>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CancelOrderHandler> _logger;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        ILogger<CancelOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Handle(
        CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Order.Order>(o => o.Id == request.OrderId && o.UserId == request.UserId);
        var order = await _orderRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException("Order not found");

        try
        {
            order.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new ResultFailureException(ex.Message);
        }

        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Cancelled order {OrderId} for user {UserId}",
            request.OrderId,
            request.UserId);
    }
}

