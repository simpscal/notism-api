using MediatR;

namespace Notism.Application.Food.AdminAddCategory;

public record AdminAddCategoryRequest : IRequest<AdminAddCategoryResponse>
{
    public required string Name { get; set; }
}