using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Order;
using Notism.Domain.Payment.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminOrdersForTable;

public class AdminOrdersForTableHandler : IRequestHandler<AdminOrdersForTableRequest, AdminOrdersForTableResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminOrdersForTableHandler> _logger;

    public AdminOrdersForTableHandler(
        IOrderRepository orderRepository,
        ILogger<AdminOrdersForTableHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<AdminOrdersForTableResponse> Handle(
        AdminOrdersForTableRequest request,
        CancellationToken cancellationToken)
    {
        PaymentStatus? paymentStatus = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus) &&
            request.PaymentStatus.ExistInEnum<PaymentStatus>())
        {
            paymentStatus = request.PaymentStatus.ToEnum<PaymentStatus>();
        }

        var specification = new AdminOrdersForTableSpecification(
            request.Keyword,
            request.SortBy,
            request.SortOrder,
            paymentStatus);

        var pagedResult = await _orderRepository.FilterPagedByExpressionAsync(specification, request);
        var items = pagedResult.Items.Select(MapToResponse).ToList();

        _logger.LogInformation("Retrieved {Count} orders for table view", items.Count);

        return new AdminOrdersForTableResponse
        {
            TotalCount = pagedResult.TotalCount,
            Items = items,
        };
    }

    private static AdminOrdersForTableOrderResponse MapToResponse(Domain.Order.Order order)
    {
        var user = order.User;

        return new AdminOrdersForTableOrderResponse
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