using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.UpdateDeliveryStatus;

public class UpdateDeliveryStatusHandler : IRequestHandler<UpdateDeliveryStatusRequest, UpdateDeliveryStatusResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<UpdateDeliveryStatusHandler> _logger;

    public UpdateDeliveryStatusHandler(
        IOrderRepository orderRepository,
        ILogger<UpdateDeliveryStatusHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<UpdateDeliveryStatusResponse> Handle(
        UpdateDeliveryStatusRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Order.Order>(o => o.Id == request.OrderId && o.UserId == request.UserId);
        var order = await _orderRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException("Order not found");

        var deliveryStatus = request.DeliveryStatus.ToEnum<DeliveryStatus>();

        order.UpdateDeliveryStatus(deliveryStatus);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated delivery status of order {OrderId} to {Status} for user {UserId}",
            request.OrderId,
            request.DeliveryStatus,
            request.UserId);

        return new UpdateDeliveryStatusResponse
        {
            OrderId = order.Id,
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            UpdatedAt = order.UpdatedAt,
        };
    }
}