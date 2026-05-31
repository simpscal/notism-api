namespace Notism.Application.Food.AdminAddCustomisationGroup;

public class AdminAddCustomisationGroupResponse
{
    public Guid Id { get; set; }
    public Guid FoodId { get; set; }
    public required string Label { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
}
