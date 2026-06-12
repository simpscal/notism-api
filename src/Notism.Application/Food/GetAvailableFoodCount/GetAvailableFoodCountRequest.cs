using MediatR;

namespace Notism.Application.Food.GetAvailableFoodCount;

public record GetAvailableFoodCountRequest : IRequest<GetAvailableFoodCountResponse>
{
    public string? Category { get; set; }
}
