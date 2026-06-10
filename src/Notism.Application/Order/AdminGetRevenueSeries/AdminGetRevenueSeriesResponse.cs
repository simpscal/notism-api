namespace Notism.Application.Order.AdminGetRevenueSeries;

public sealed record AdminGetRevenueSeriesResponse
{
    public required string Granularity { get; init; }
    public required IReadOnlyList<RevenueSeriesPoint> Points { get; init; }

    public static AdminGetRevenueSeriesResponse FromPoints(
        string? granularity,
        IReadOnlyList<RevenueSeriesPoint> points)
    {
        return new AdminGetRevenueSeriesResponse
        {
            // Client hint echoed back verbatim (empty string when omitted).
            Granularity = granularity ?? string.Empty,
            Points = points,
        };
    }
}

public sealed record RevenueSeriesPoint
{
    public required string Period { get; init; }
    public required decimal Revenue { get; init; }
}