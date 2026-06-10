using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Order.Repositories;

namespace Notism.Application.Order.AdminGetOrderStatusSummary;

public class AdminGetOrderStatusSummaryHandler
    : IRequestHandler<AdminGetOrderStatusSummaryRequest, AdminGetOrderStatusSummaryResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminGetOrderStatusSummaryHandler> _logger;

    public AdminGetOrderStatusSummaryHandler(
        IOrderRepository orderRepository,
        ILogger<AdminGetOrderStatusSummaryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<AdminGetOrderStatusSummaryResponse> Handle(
        AdminGetOrderStatusSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var counts = await _orderRepository.GetDeliveryStatusBucketCountsAsync();

        _logger.LogInformation(
            "Retrieved order status summary: New={New}, InProgress={InProgress}, Completed={Completed}",
            counts.New,
            counts.InProgress,
            counts.Completed);

        return AdminGetOrderStatusSummaryResponse.FromDomain(counts);
    }
}