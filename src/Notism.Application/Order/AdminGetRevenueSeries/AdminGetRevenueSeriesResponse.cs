using Notism.Domain.Order.Repositories;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public sealed record AdminGetRevenueSeriesResponse
{
    public required string Granularity { get; init; }
    public required IReadOnlyList<RevenueSeriesPoint> Points { get; init; }

    public static AdminGetRevenueSeriesResponse FromPoints(
        RevenuePeriodGranularity granularity,
        IReadOnlyList<RevenueSeriesPoint> points)
    {
        return new AdminGetRevenueSeriesResponse
        {
            Granularity = granularity.ToCamelCase(),
            Points = points,
        };
    }
}

public sealed record RevenueSeriesPoint
{
    public required string Period { get; init; }
    public required decimal Revenue { get; init; }
}