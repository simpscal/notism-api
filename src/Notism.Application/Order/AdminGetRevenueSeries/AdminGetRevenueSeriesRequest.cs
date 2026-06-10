using MediatR;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public sealed record AdminGetRevenueSeriesRequest : IRequest<AdminGetRevenueSeriesResponse>
{
    public string? Granularity { get; set; }
}