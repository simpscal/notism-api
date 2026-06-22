using MediatR;

using Notism.Application.Common.Abstractions;
using Notism.Application.Common.Constants;

namespace Notism.Application.Food.AdminDeleteFood;

public record AdminDeleteFoodRequest : IRequest, ICacheInvalidatingRequest
{
    public IEnumerable<string> CacheTagsToEvict => [CacheTagConstants.Foods];

    public Guid FoodId { get; init; }
}