using MediatR;

namespace Notism.Application.Food.AdminGetCategoryDetail;

public record AdminGetCategoryDetailRequest : IRequest<AdminGetCategoryDetailResponse>
{
    public Guid CategoryId { get; set; }
}