using Notism.Domain.Common;

namespace Notism.Domain.Payment;

public class BankingCheckout : AggregateRoot
{
    public Guid UserId { get; private set; }
    public List<Guid> CartItemIds { get; private set; } = new();
    public decimal TotalAmount { get; private set; }
    public bool IsUsed { get; private set; }

    private BankingCheckout(Guid userId, List<Guid> cartItemIds, decimal totalAmount)
    {
        UserId = userId;
        CartItemIds = cartItemIds;
        TotalAmount = totalAmount;
        IsUsed = false;
    }

    public static BankingCheckout Create(Guid userId, List<Guid> cartItemIds, decimal totalAmount)
    {
        return new BankingCheckout(userId, cartItemIds, totalAmount);
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private BankingCheckout()
    {
    }
}
