using MediatR;

using Notism.Shared.Models;

namespace Notism.Application.Food.GetFoods;

public record GetFoodsRequest : FilterParams, IRequest<GetFoodsResponse>
{
    public string? Category { get; set; }
    public bool? IsAvailable { get; set; }
}