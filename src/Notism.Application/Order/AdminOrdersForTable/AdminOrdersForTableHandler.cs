using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Mappers;
using Notism.Application.Order.Models;
using Notism.Domain.Order;

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
        var specification = new AdminOrdersForTableSpecification(
            request.Keyword,
            request.SortBy,
            request.SortOrder);

        var pagedResult = await _orderRepository.FilterPagedByExpressionAsync(specification, request);
        var items = pagedResult.Items.Select(order => AdminOrderMapper.ToAdminOrderResponse(order, order.User)).ToList();

        _logger.LogInformation("Retrieved {Count} orders for table view", items.Count);

        return new AdminOrdersForTableResponse
        {
            TotalCount = pagedResult.TotalCount,
            Items = items,
        };
    }
}