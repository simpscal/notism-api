using MediatR;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public sealed record AdminGetRevenueSeriesRequest : IRequest<AdminGetRevenueSeriesResponse>
{
    /// <summary>
    /// Gets or sets ordered, strictly ascending UTC boundary instants (n+1 of them) defining the
    /// n half-open buckets [boundaries[i], boundaries[i+1]). Supplied by the client.
    /// </summary>
    public List<DateTime> Boundaries { get; set; } = new();

    /// <summary>Gets or sets one label per bucket (n labels). Echoed back as each point's period.</summary>
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Gets or sets optional client hint (e.g. year / month / day). Echoed back verbatim in the
    /// response; carries NO server semantics.
    /// </summary>
    public string? Granularity { get; set; }
}