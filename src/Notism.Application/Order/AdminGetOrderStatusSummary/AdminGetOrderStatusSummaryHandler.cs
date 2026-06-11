using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;

namespace Notism.Application.Order.AdminGetOrderStatusSummary;

public class AdminGetOrderStatusSummaryHandler
    : IRequestHandler<AdminGetOrderStatusSummaryRequest, AdminGetOrderStatusSummaryResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetOrderStatusSummaryHandler> _logger;

    public AdminGetOrderStatusSummaryHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetOrderStatusSummaryHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminGetOrderStatusSummaryResponse> Handle(
        AdminGetOrderStatusSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var counts = await new GetDeliveryStatusBucketCountsQuery(_readDbContext)
            .ExecuteAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved order status summary: New={New}, InProgress={InProgress}, Completed={Completed}",
            counts.New,
            counts.InProgress,
            counts.Completed);

        return AdminGetOrderStatusSummaryResponse.FromDomain(counts);
    }
}