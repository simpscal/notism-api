using Notism.Domain.Cart;
using Notism.Domain.Food;

namespace Notism.Application.Cart.Common;

public record CartItemCustomisationResponse
{
    public Guid? GroupId { get; set; }
    public required string GroupLabel { get; set; }
    public Guid? OptionId { get; set; }
    public required string OptionLabel { get; set; }
    public decimal? Surcharge { get; set; }
    public List<CartItemAvailableOptionResponse> AvailableOptions { get; set; } = new();

    public static CartItemCustomisationResponse FromDomain(
        CartItemCustomisation customisation,
        List<CartItemAvailableOptionResponse> availableOptions)
    {
        return new CartItemCustomisationResponse
        {
            GroupId = customisation.CustomisationGroupId,
            GroupLabel = customisation.GroupLabel,
            OptionId = customisation.CustomisationOptionId,
            OptionLabel = customisation.OptionLabel,
            Surcharge = customisation.Surcharge,
            AvailableOptions = availableOptions,
        };
    }

    public static CartItemCustomisationResponse FromDomain(
        CartItemCustomisation customisation,
        FoodCustomisationGroup? group)
    {
        var availableOptions = group?.Options
            .Select(CartItemAvailableOptionResponse.FromDomain)
            .ToList() ?? new List<CartItemAvailableOptionResponse>();

        var optionStillExists = group?.Options.Any(o => o.Id == customisation.CustomisationOptionId) ?? false;

        return new CartItemCustomisationResponse
        {
            GroupId = customisation.CustomisationGroupId,
            GroupLabel = customisation.GroupLabel,
            OptionId = optionStillExists ? customisation.CustomisationOptionId : null,
            OptionLabel = optionStillExists ? customisation.OptionLabel : "Option no longer available",
            Surcharge = optionStillExists ? customisation.Surcharge : null,
            AvailableOptions = availableOptions,
        };
    }
}
