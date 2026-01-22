using Notism.Domain.Common;
using Notism.Domain.Food.Enums;
using Notism.Domain.Food.Events;

namespace Notism.Domain.Food;

public class Food : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string FileKey { get; private set; }
    public FoodCategory Category { get; private set; }
    public bool IsAvailable { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; }
    public decimal? DiscountPrice { get; private set; }
    public int StockQuantity { get; private set; }

    private Food(
        string name,
        string description,
        decimal price,
        string fileKey,
        FoodCategory category,
        QuantityUnit quantityUnit,
        int stockQuantity,
        decimal? discountPrice = null)
    {
        Name = name;
        Description = description;
        Price = price;
        FileKey = fileKey;
        Category = category;
        IsAvailable = true;
        QuantityUnit = quantityUnit;
        DiscountPrice = discountPrice;
        StockQuantity = stockQuantity;

        AddDomainEvent(new FoodCreatedEvent(Id, Name, Category));
    }

    public static Food Create(
        string name,
        string description,
        decimal price,
        string fileKey,
        FoodCategory category,
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

        return new Food(name, description, price, fileKey, category, quantityUnit, stockQuantity, discountPrice);
    }

    public void Update(
        string name,
        string description,
        decimal price,
        string fileKey,
        FoodCategory category,
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

        Name = name;
        Description = description;
        Price = price;
        FileKey = fileKey;
        Category = category;
        QuantityUnit = quantityUnit;
        StockQuantity = stockQuantity;
        DiscountPrice = discountPrice;
        UpdatedAt = DateTime.UtcNow;

        ClearDomainEvents();
        AddDomainEvent(new FoodUpdatedEvent(Id, Name, Category));
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
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

    // Required for EF Core
    private Food()
    {
        Name = string.Empty;
        Description = string.Empty;
        FileKey = string.Empty;
    }

    public decimal GetEffectivePrice() => DiscountPrice ?? Price;

    public bool HasDiscount() => DiscountPrice.HasValue && DiscountPrice.Value < Price;
}