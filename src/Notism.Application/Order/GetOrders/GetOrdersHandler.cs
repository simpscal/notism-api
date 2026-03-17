using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Order.Mappers;
using Notism.Domain.Common.Specifications;
using Notism.Domain.Order;
using Notism.Domain.Order.Enums;

namespace Notism.Application.Order.GetOrders;

public class GetOrdersHandler : IRequestHandler<GetOrdersRequest, GetOrdersResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(
        IOrderRepository orderRepository,
        IStorageService storageService,
        ILogger<GetOrdersHandler> logger)
    {
        _orderRepository = orderRepository;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<GetOrdersResponse> Handle(
        GetOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var specification = new FilterSpecification<Domain.Order.Order>(
                o => o.UserId == request.UserId && o.DeliveryStatus != DeliveryStatus.Cancelled)
            .Include("Items.Food.Images")
            .Include(o => o.StatusHistory);
        var orders = await _orderRepository.FilterByExpressionAsync(specification);

        var orderResponses = orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(order => OrderMapper.ToResponse(order, _storageService))
            .ToList();

        _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orderResponses.Count, request.UserId);

        return new GetOrdersResponse
        {
            Orders = orderResponses,
        };
    }
}