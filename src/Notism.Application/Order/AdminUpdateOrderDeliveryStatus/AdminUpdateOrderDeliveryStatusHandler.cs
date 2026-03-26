using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Order.Mappers;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Shared.Exceptions;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminUpdateOrderDeliveryStatus;

public class AdminUpdateOrderDeliveryStatusHandler : IRequestHandler<AdminUpdateOrderDeliveryStatusRequest, AdminUpdateOrderDeliveryStatusResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminUpdateOrderDeliveryStatusHandler> _logger;
    private readonly IMessages _messages;

    public AdminUpdateOrderDeliveryStatusHandler(
        IOrderRepository orderRepository,
        ILogger<AdminUpdateOrderDeliveryStatusHandler> logger,
        IMessages messages)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _messages = messages;
    }

    public async Task<AdminUpdateOrderDeliveryStatusResponse> Handle(
        AdminUpdateOrderDeliveryStatusRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new AdminUpdateOrderDeliveryStatusSpecification(request.OrderId);
        var order = await _orderRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException(_messages.OrderNotFound);

        var deliveryStatus = request.DeliveryStatus.ToEnum<DeliveryStatus>();

        order.UpdateDeliveryStatus(deliveryStatus);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Updated delivery status of order {OrderId} to {Status}",
            request.OrderId,
            request.DeliveryStatus);

        var adminOrder = AdminOrderMapper.ToAdminOrderResponse(order, order.User);
        return new AdminUpdateOrderDeliveryStatusResponse(adminOrder);
    }
}