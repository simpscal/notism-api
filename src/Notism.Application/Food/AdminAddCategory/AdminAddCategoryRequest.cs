using MediatR;

using Notism.Application.Common.Behaviors;
using Notism.Application.Common.Constants;
namespace Notism.Application.Food.AdminAddCategory;

public record AdminAddCategoryRequest : IRequest<AdminAddCategoryResponse>, ICacheInvalidatingRequest
{
    public IEnumerable<string> CacheTagsToEvict => [CacheTagConstants.Categories, CacheTagConstants.Foods];

    public required string Name { get; set; }
}