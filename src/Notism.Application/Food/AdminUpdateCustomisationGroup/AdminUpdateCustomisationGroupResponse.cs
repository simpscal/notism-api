namespace Notism.Application.Food.AdminUpdateCustomisationGroup;

public class AdminUpdateCustomisationGroupResponse
{
    public Guid Id { get; set; }
    public Guid FoodId { get; set; }
    public required string Label { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}
