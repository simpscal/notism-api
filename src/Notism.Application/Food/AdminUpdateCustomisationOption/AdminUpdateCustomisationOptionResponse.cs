using Notism.Domain.Food;

namespace Notism.Application.Food.AdminUpdateCustomisationOption;

public class AdminUpdateCustomisationOptionResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public required string Label { get; set; }
    public decimal? Surcharge { get; set; }
    public int DisplayOrder { get; set; }

    public static AdminUpdateCustomisationOptionResponse FromDomain(FoodCustomisationOption option)
    {
        return new AdminUpdateCustomisationOptionResponse
        {
            Id = option.Id,
            GroupId = option.GroupId,
            Label = option.Label,
            Surcharge = option.Surcharge,
            DisplayOrder = option.DisplayOrder,
        };
    }
}
