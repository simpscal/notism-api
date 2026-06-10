using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Order.Repositories;
using Notism.Shared.Utilities;

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
        var (startUtc, endUtc) = DayWindow.HoChiMinhDay(DateTime.UtcNow);

        var aggregate = await _orderRepository.GetWindowAggregateAsync(startUtc, endUtc);

        _logger.LogInformation(
            "Retrieved today's sales for window [{StartUtc:o}, {EndUtc:o}): Revenue={Revenue}, OrderCount={OrderCount}",
            startUtc,
            endUtc,
            aggregate.Revenue,
            aggregate.OrderCount);

        return AdminGetTodaySalesResponse.FromDomain(aggregate);
    }
}