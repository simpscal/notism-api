using Notism.Domain.Common;

namespace Notism.Domain.Order;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid FoodId { get; private set; }
    public string FoodName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? DiscountPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }
    public Domain.Food.Food Food { get; private set; } = null!;
    public Order Order { get; private set; } = null!;

    private OrderItem(
        Guid orderId,
        Guid foodId,
        string foodName,
        decimal unitPrice,
        decimal? discountPrice,
        int quantity)
    {
        OrderId = orderId;
        FoodId = foodId;
        FoodName = foodName;
        UnitPrice = unitPrice;
        DiscountPrice = discountPrice;
        Quantity = quantity;
        TotalPrice = (discountPrice.HasValue && discountPrice.Value > 0 ? discountPrice.Value : unitPrice) * quantity;
    }

    public static OrderItem Create(
        Guid orderId,
        Guid foodId,
        string foodName,
        decimal unitPrice,
        decimal? discountPrice,
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentException("Unit price must be greater than zero", nameof(unitPrice));
        }

        return new OrderItem(orderId, foodId, foodName, unitPrice, discountPrice, quantity);
    }

    private OrderItem()
    {
        FoodName = string.Empty;
    }
}