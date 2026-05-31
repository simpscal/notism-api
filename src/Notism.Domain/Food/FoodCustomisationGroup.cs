using Notism.Domain.Common;

namespace Notism.Domain.Food;

public class FoodCustomisationGroup : Entity
{
    public Guid FoodId { get; private set; }
    public string Label { get; private set; }
    public bool IsRequired { get; private set; }
    public int DisplayOrder { get; private set; }

    public Food Food { get; private set; } = null!;

    private readonly List<FoodCustomisationOption> _options = new();
    public IReadOnlyCollection<FoodCustomisationOption> Options => _options.AsReadOnly();

    private FoodCustomisationGroup(
        Guid foodId,
        string label,
        bool isRequired,
        int displayOrder)
    {
        FoodId = foodId;
        Label = label;
        IsRequired = isRequired;
        DisplayOrder = displayOrder;
    }

    public static FoodCustomisationGroup Create(
        Guid foodId,
        string label,
        bool isRequired,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Group label cannot be empty", nameof(label));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
        }

        return new FoodCustomisationGroup(foodId, label, isRequired, displayOrder);
    }

    public void AddOption(string label, decimal? surcharge, int displayOrder)
    {
        var option = FoodCustomisationOption.Create(Id, label, surcharge, displayOrder);
        _options.Add(option);
    }

    private FoodCustomisationGroup()
    {
        Label = string.Empty;
    }
}
