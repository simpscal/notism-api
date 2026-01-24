using Notism.Domain.Common;

namespace Notism.Domain.Food;

public class FoodImage : Entity
{
    public Guid FoodId { get; private set; }
    public string FileKey { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? AltText { get; private set; }

    public Food Food { get; private set; } = null!;

    private FoodImage(
        Guid foodId,
        string fileKey,
        int displayOrder,
        string? altText = null)
    {
        FoodId = foodId;
        FileKey = fileKey;
        DisplayOrder = displayOrder;
        AltText = altText;
    }

    public static FoodImage Create(
        Guid foodId,
        string fileKey,
        int displayOrder,
        string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("File key cannot be empty", nameof(fileKey));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
        }

        return new FoodImage(foodId, fileKey, displayOrder, altText);
    }

    public void Update(string fileKey, int displayOrder, string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("File key cannot be empty", nameof(fileKey));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
        }

        FileKey = fileKey;
        DisplayOrder = displayOrder;
        AltText = altText;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentException("Display order cannot be negative", nameof(displayOrder));
        }

        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    private FoodImage()
    {
        FileKey = string.Empty;
    }
}

