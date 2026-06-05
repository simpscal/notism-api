using MediatR;

namespace Notism.Application.Food.AdminUpdateCustomisationGroup;

public record AdminUpdateCustomisationGroupRequest : IRequest<AdminUpdateCustomisationGroupResponse>
{
    public Guid FoodId { get; set; }
    public Guid GroupId { get; set; }
    public string? Label { get; set; }
    public bool? IsRequired { get; set; }
    public int? DisplayOrder { get; set; }
}
