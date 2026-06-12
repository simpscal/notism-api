using MediatR;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public sealed record AdminGetRevenueSeriesRequest : IRequest<AdminGetRevenueSeriesResponse>
{
    public List<DateTime> Boundaries { get; set; } = new();
    public List<string> Labels { get; set; } = new();
    public string? Granularity { get; set; }
}