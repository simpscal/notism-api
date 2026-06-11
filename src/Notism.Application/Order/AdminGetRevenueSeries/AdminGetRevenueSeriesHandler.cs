using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Persistence;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesHandler
    : IRequestHandler<AdminGetRevenueSeriesRequest, AdminGetRevenueSeriesResponse>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<AdminGetRevenueSeriesHandler> _logger;

    public AdminGetRevenueSeriesHandler(
        IReadDbContext readDbContext,
        ILogger<AdminGetRevenueSeriesHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<AdminGetRevenueSeriesResponse> Handle(
        AdminGetRevenueSeriesRequest request,
        CancellationToken cancellationToken)
    {
        // The validator guarantees >= 2 strictly-ascending boundaries and
        // labels.Count == boundaries.Count - 1, so bucketCount >= 1.
        var bucketCount = request.Boundaries.Count - 1;

        var populated = await new GetRevenueByBucketsQuery(_readDbContext)
            .ExecuteAsync(request.Boundaries, cancellationToken);

        // The repository returns only buckets that contain Paid orders. Zero-fill the
        // absent indices into a dense, boundary-ordered series; this handler owns the
        // zero-fill, never the SQL.
        var revenueByBucket = populated.ToDictionary(b => b.BucketIndex, b => b.Revenue);

        var points = Enumerable.Range(0, bucketCount)
            .Select(i => new RevenueSeriesPoint
            {
                Period = request.Labels[i],
                Revenue = revenueByBucket.TryGetValue(i, out var revenue) ? revenue : 0m,
            })
            .ToList();

        _logger.LogInformation(
            "Retrieved revenue series: {PopulatedCount} populated bucket(s), {PointCount} dense point(s)",
            populated.Count,
            points.Count);

        return AdminGetRevenueSeriesResponse.FromPoints(request.Granularity, points);
    }
}