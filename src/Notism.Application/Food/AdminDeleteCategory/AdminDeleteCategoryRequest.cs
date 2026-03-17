using MediatR;

namespace Notism.Application.Food.AdminDeleteCategory;

public record AdminDeleteCategoryRequest : IRequest
{
    public Guid CategoryId { get; set; }
}