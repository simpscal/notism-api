using Notism.Domain.Common;

namespace Notism.Domain.Food;

public class FoodCustomisationOption : Entity
{
    public Guid GroupId { get; private set; }
    public string Label { get; private set; }
    public decimal? Surcharge { get; private set; }
    public int DisplayOrder { get; private set; }

    public FoodCustomisationGroup Group { get; private set; } = null!;

    private FoodCustomisationOption(
        Guid groupId,
        string label,
        decimal? surcharge,
        int displayOrder)
    {
        GroupId = groupId;
        Label = label;
        Surcharge = surcharge;
        DisplayOrder = displayOrder;
    }

    public static FoodCustomisationOption Create(
        Guid groupId,
        string label,
        decimal? surcharge,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Option label cannot be empty", nameof(label));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
        }

        return new FoodCustomisationOption(groupId, label, surcharge, displayOrder);
    }

    public void Update(string? label = null, decimal? surcharge = null, int? displayOrder = null)
    {
        if (label != null)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Option label cannot be empty", nameof(label));
            }

            Label = label;
        }

        if (surcharge.HasValue)
        {
            Surcharge = surcharge.Value;
        }

        if (displayOrder.HasValue)
        {
            if (displayOrder.Value < 0)
            {
                throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
            }

            DisplayOrder = displayOrder.Value;
        }
    }

    private FoodCustomisationOption()
    {
        Label = string.Empty;
    }
}
