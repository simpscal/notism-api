using MediatR;

using Notism.Application.Common.Behaviors;
using Notism.Application.Common.Constants;
namespace Notism.Application.Food.AdminDeleteCategory;

public record AdminDeleteCategoryRequest : IRequest, ICacheInvalidatingRequest
{
    public IEnumerable<string> CacheTagsToEvict => [CacheTagConstants.Categories, CacheTagConstants.Foods];

    public Guid CategoryId { get; set; }
}