using MediatR;

using Notism.Application.Common.Caching;
namespace Notism.Application.Food.AdminAddCategory;

public record AdminAddCategoryRequest : IRequest<AdminAddCategoryResponse>, ICacheInvalidatingRequest
{
    public IEnumerable<string> CacheTagsToEvict => [CacheTagConstants.Categories, CacheTagConstants.Foods];

    public required string Name { get; set; }
}