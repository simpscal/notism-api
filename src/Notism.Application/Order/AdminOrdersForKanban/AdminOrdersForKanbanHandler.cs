using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Order.Mappers;
using Notism.Application.Order.Models;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminOrdersForKanban;

public class AdminOrdersForKanbanHandler : IRequestHandler<AdminOrdersForKanbanRequest, AdminOrdersForKanbanResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminOrdersForKanbanHandler> _logger;

    public AdminOrdersForKanbanHandler(
        IOrderRepository orderRepository,
        ILogger<AdminOrdersForKanbanHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<AdminOrdersForKanbanResponse> Handle(
        AdminOrdersForKanbanRequest request,
        CancellationToken cancellationToken)
    {
        var deliveryStatus = request.Status.ToEnum<DeliveryStatus>();

        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            paymentStatus = request.PaymentStatus.ToEnum<PaymentStatus>();
        }

        var specification = new AdminOrdersForKanbanSpecification(deliveryStatus, paymentStatus);
        var pagedResult = await _orderRepository.FilterPagedByExpressionAsync(specification, request);

        var items = pagedResult.Items.Select(order => AdminOrderMapper.ToAdminOrderResponse(order, order.User)).ToList();

        _logger.LogInformation(
            "Retrieved {Count} orders for kanban view with status {Status}",
            items.Count,
            request.Status);

        return new AdminOrdersForKanbanResponse
        {
            TotalCount = pagedResult.TotalCount,
            Items = items,
        };
    }
}