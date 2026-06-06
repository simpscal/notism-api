using Notism.Domain.Food;

namespace Notism.Application.Cart.Common;

public sealed record CartItemAvailableOptionResponse
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public decimal? Surcharge { get; set; }

    public static CartItemAvailableOptionResponse FromDomain(FoodCustomisationOption option)
    {
        return new CartItemAvailableOptionResponse
        {
            Id = option.Id,
            Label = option.Label,
            Surcharge = option.Surcharge,
        };
    }
}