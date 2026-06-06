using MediatR;

using Microsoft.Extensions.Logging;

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

        var items = pagedResult.Items.Select(MapToResponse).ToList();

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

    private static AdminOrdersForKanbanOrderResponse MapToResponse(Domain.Order.Order order)
    {
        var user = order.User;

        return new AdminOrdersForKanbanOrderResponse
        {
            Id = order.Id,
            SlugId = order.SlugId,
            UserId = order.UserId,
            UserEmail = user?.Email.Value ?? string.Empty,
            UserName = user?.FullName ?? string.Empty,
            TotalAmount = order.TotalAmount,
            DeliveryStatus = order.DeliveryStatus.GetStringValue(),
            PaymentStatus = order.PaymentStatus.GetStringValue(),
            PaidAt = order.PaidAt,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            TotalItems = order.Items.Count,
            DeliveryNotes = order.DeliveryNotes,
        };
    }
}