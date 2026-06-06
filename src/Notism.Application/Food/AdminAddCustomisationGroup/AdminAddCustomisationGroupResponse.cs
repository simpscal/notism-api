using Notism.Domain.Food;

namespace Notism.Application.Food.AdminAddCustomisationGroup;

public class AdminAddCustomisationGroupResponse
{
    public Guid Id { get; set; }
    public Guid FoodId { get; set; }
    public required string Label { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public static AdminAddCustomisationGroupResponse FromDomain(FoodCustomisationGroup group)
    {
        return new AdminAddCustomisationGroupResponse
        {
            Id = group.Id,
            FoodId = group.FoodId,
            Label = group.Label,
            IsRequired = group.IsRequired,
            DisplayOrder = group.DisplayOrder,
        };
    }
}
