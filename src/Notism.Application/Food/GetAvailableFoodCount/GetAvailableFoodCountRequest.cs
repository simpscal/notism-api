using MediatR;

namespace Notism.Application.Food.GetAvailableFoodCount;

public record GetAvailableFoodCountRequest : IRequest<GetAvailableFoodCountResponse>
{
    /// <summary>Optional category name; when set, counts only foods in that category.</summary>
    public string? Category { get; set; }
}
