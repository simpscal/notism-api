using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Order.Repositories;

namespace Notism.Application.Order.AdminGetTodaySales;

public class AdminGetTodaySalesHandler
    : IRequestHandler<AdminGetTodaySalesRequest, AdminGetTodaySalesResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminGetTodaySalesHandler> _logger;

    public AdminGetTodaySalesHandler(
        IOrderRepository orderRepository,
        ILogger<AdminGetTodaySalesHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<AdminGetTodaySalesResponse> Handle(
        AdminGetTodaySalesRequest request,
        CancellationToken cancellationToken)
    {
        // The client supplies both UTC boundaries; pass them straight through. The
        // server performs NO window derivation and is fully time-zone agnostic.
        var aggregate = await _orderRepository.GetWindowAggregateAsync(request.StartUtc, request.EndUtc);

        _logger.LogInformation(
            "Retrieved sales for window [{StartUtc:o}, {EndUtc:o}): Revenue={Revenue}, OrderCount={OrderCount}",
            request.StartUtc,
            request.EndUtc,
            aggregate.Revenue,
            aggregate.OrderCount);

        return AdminGetTodaySalesResponse.FromDomain(aggregate);
    }
}