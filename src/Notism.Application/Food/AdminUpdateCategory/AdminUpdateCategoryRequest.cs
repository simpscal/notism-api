using MediatR;

namespace Notism.Application.Food.AdminUpdateCategory;

public record AdminUpdateCategoryRequest : IRequest<AdminUpdateCategoryResponse>
{
    public Guid CategoryId { get; set; }
    public required string Name { get; set; }
}