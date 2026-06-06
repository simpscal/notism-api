using Notism.Domain.Food;

namespace Notism.Application.Food.AdminUpdateCustomisationGroup;

public sealed record AdminUpdateCustomisationGroupResponse
{
    public Guid Id { get; set; }
    public Guid FoodId { get; set; }
    public required string Label { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }

    public static AdminUpdateCustomisationGroupResponse FromDomain(FoodCustomisationGroup group)
    {
        return new AdminUpdateCustomisationGroupResponse
        {
            Id = group.Id,
            FoodId = group.FoodId,
            Label = group.Label,
            IsRequired = group.IsRequired,
            DisplayOrder = group.DisplayOrder,
        };
    }
}