using Notism.Domain.Food;

namespace Notism.Application.Food.AdminAddCustomisationOption;

public sealed record AdminAddCustomisationOptionResponse
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public required string Label { get; set; }
    public decimal? Surcharge { get; set; }
    public int DisplayOrder { get; set; }

    public static AdminAddCustomisationOptionResponse FromDomain(FoodCustomisationOption option)
    {
        return new AdminAddCustomisationOptionResponse
        {
            Id = option.Id,
            GroupId = option.GroupId,
            Label = option.Label,
            Surcharge = option.Surcharge,
            DisplayOrder = option.DisplayOrder,
        };
    }
}