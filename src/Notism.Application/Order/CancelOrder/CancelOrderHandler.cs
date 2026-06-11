using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Domain.Order.Repositories;
using Notism.Shared.Exceptions;

namespace Notism.Application.Order.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderRequest>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CancelOrderHandler> _logger;
    private readonly IMessages _messages;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        ILogger<CancelOrderHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task Handle(
        CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetForUpdateAsync(
                o => o.Id == request.OrderId && o.UserId == request.UserId)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

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