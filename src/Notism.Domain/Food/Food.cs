using Notism.Domain.Common;
using Notism.Domain.Food.Enums;
using Notism.Domain.Food.Events;

namespace Notism.Domain.Food;

public class Food : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public bool IsAvailable { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; }
    public decimal? DiscountPrice { get; private set; }
    public int StockQuantity { get; private set; }

    private readonly List<FoodImage> _images = new();
    public IReadOnlyCollection<FoodImage> Images => _images.AsReadOnly();

    private Food(
        string name,
        string description,
        decimal price,
        Guid? categoryId,
        QuantityUnit quantityUnit,
        int stockQuantity,
        decimal? discountPrice = null)
    {
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        IsAvailable = true;
        QuantityUnit = quantityUnit;
        DiscountPrice = discountPrice;
        StockQuantity = stockQuantity;

        AddDomainEvent(new FoodCreatedEvent(Id, Name, categoryId));
    }

    public static Food Create(
        string name,
        string description,
        decimal price,
        Guid categoryId,
        QuantityUnit quantityUnit,
        int stockQuantity,
        decimal? discountPrice = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Food name cannot be empty", nameof(name));
        }

        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than zero", nameof(price));
        }

        if (discountPrice.HasValue && discountPrice.Value >= price)
        {
            throw new ArgumentException("Discount price must be less than the original price", nameof(discountPrice));
        }

        return new Food(name, description, price, categoryId, quantityUnit, stockQuantity, discountPrice);
    }

    public void Update(
        string? name = null,
        string? description = null,
        decimal? price = null,
        Guid? categoryId = null,
        QuantityUnit? quantityUnit = null,
        int? stockQuantity = null,
        decimal? discountPrice = null)
    {
        var hasAny = name != null || description != null || price.HasValue || categoryId.HasValue
            || quantityUnit.HasValue || stockQuantity.HasValue || discountPrice.HasValue;
        if (!hasAny)
        {
            throw new ArgumentException("At least one update parameter must be provided.");
        }

        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Food name cannot be empty", nameof(name));
            }

            Name = name;
        }

        if (description != null)
        {
            Description = description;
        }

        if (price.HasValue)
        {
            if (price.Value <= 0)
            {
                throw new ArgumentException("Price must be greater than zero", nameof(price));
            }

            Price = price.Value;
        }

        if (categoryId.HasValue)
        {
            CategoryId = categoryId;
        }

        if (quantityUnit.HasValue)
        {
            QuantityUnit = quantityUnit.Value;
        }

        if (stockQuantity.HasValue)
        {
            StockQuantity = stockQuantity.Value;
        }

        if (discountPrice.HasValue)
        {
            var priceToCompare = Price;
            if (discountPrice.Value >= priceToCompare)
            {
                throw new ArgumentException("Discount price must be less than the original price", nameof(discountPrice));
            }

            DiscountPrice = discountPrice;
        }

        UpdatedAt = DateTime.UtcNow;

        ClearDomainEvents();
        AddDomainEvent(new FoodUpdatedEvent(Id, Name, CategoryId));
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        }

        StockQuantity = quantity;
        IsAvailable = quantity > 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity to deduct must be positive", nameof(quantity));
        }

        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException("Insufficient stock");
        }

        StockQuantity -= quantity;
        if (StockQuantity == 0)
        {
            IsAvailable = false;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    private Food()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public decimal GetEffectivePrice() => DiscountPrice ?? Price;

    public bool HasDiscount() => DiscountPrice.HasValue && DiscountPrice.Value < Price;

    public void AddImage(string fileKey, int displayOrder, string? altText = null)
    {
        var image = FoodImage.Create(Id, fileKey, displayOrder, altText);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            _images.Remove(image);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ClearImages()
    {
        _images.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateImages(IEnumerable<(string FileKey, int DisplayOrder, string? AltText)> images)
    {
        _images.Clear();
        foreach (var (fileKey, displayOrder, altText) in images)
        {
            _images.Add(FoodImage.Create(Id, fileKey, displayOrder, altText));
        }

        UpdatedAt = DateTime.UtcNow;
    }
}