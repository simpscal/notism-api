using MediatR;

namespace Notism.Application.Food.AdminAddCustomisationGroup;

public record AdminAddCustomisationGroupRequest : IRequest<AdminAddCustomisationGroupResponse>
{
    public Guid FoodId { get; set; }
    public required string Label { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}
