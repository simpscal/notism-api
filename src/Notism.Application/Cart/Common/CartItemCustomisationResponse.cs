using Notism.Domain.Cart;

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
}