using MediatR;

using Notism.Application.Common.Abstractions;
using Notism.Application.Common.Constants;

namespace Notism.Application.Food.AdminUpdateCategory;

public record AdminUpdateCategoryRequest : IRequest<AdminUpdateCategoryResponse>, ICacheInvalidatingRequest
{
    public IEnumerable<string> CacheTagsToEvict => [CacheTagConstants.Categories, CacheTagConstants.Foods];

    public Guid CategoryId { get; set; }
    public required string Name { get; set; }
}