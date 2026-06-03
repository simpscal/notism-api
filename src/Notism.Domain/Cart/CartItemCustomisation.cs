using Notism.Domain.Common;

namespace Notism.Domain.Cart;

public class CartItemCustomisation : Entity
{
    public Guid CartItemId { get; private set; }
    public Guid? CustomisationGroupId { get; private set; }
    public Guid? CustomisationOptionId { get; private set; }
    public string GroupLabel { get; private set; }
    public string OptionLabel { get; private set; }
    public decimal? Surcharge { get; private set; }

    public CartItem CartItem { get; private set; } = null!;

    private CartItemCustomisation(
        Guid cartItemId,
        Guid? customisationGroupId,
        Guid? customisationOptionId,
        string groupLabel,
        string optionLabel,
        decimal? surcharge)
    {
        CartItemId = cartItemId;
        CustomisationGroupId = customisationGroupId;
        CustomisationOptionId = customisationOptionId;
        GroupLabel = groupLabel;
        OptionLabel = optionLabel;
        Surcharge = surcharge;
    }

    public static CartItemCustomisation Create(
        Guid cartItemId,
        Guid? customisationGroupId,
        Guid? customisationOptionId,
        string groupLabel,
        string optionLabel,
        decimal? surcharge)
    {
        if (string.IsNullOrWhiteSpace(groupLabel))
        {
            throw new ArgumentException("Group label cannot be empty", nameof(groupLabel));
        }

        if (string.IsNullOrWhiteSpace(optionLabel))
        {
            throw new ArgumentException("Option label cannot be empty", nameof(optionLabel));
        }

        return new CartItemCustomisation(
            cartItemId,
            customisationGroupId,
            customisationOptionId,
            groupLabel,
            optionLabel,
            surcharge);
    }

    private CartItemCustomisation()
    {
        GroupLabel = string.Empty;
        OptionLabel = string.Empty;
    }
}
