using MediatR;

namespace Notism.Application.Order.AdminGetTodaySales;

public sealed record AdminGetTodaySalesRequest : IRequest<AdminGetTodaySalesResponse>
{
    /// <summary>Gets or sets inclusive start of the window (UTC instant, supplied by the client).</summary>
    public DateTime StartUtc { get; set; }

    /// <summary>Gets or sets exclusive end of the window (UTC instant, supplied by the client).</summary>
    public DateTime EndUtc { get; set; }
}