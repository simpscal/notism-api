using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.GetHeldRefunds;

public sealed record GetHeldRefundsResponse
{
    public required IReadOnlyList<HeldRefundResponse> Items { get; init; }

    public static GetHeldRefundsResponse FromItems(IReadOnlyList<HeldRefundResponse> items)
    {
        return new GetHeldRefundsResponse
        {
            Items = items,
        };
    }
}

public sealed record HeldRefundResponse
{
    public Guid RefundId { get; set; }
    public string OrderReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public static HeldRefundResponse FromDomain(DomainOrder order)
    {
        var refund = order.Refund!;

        return new HeldRefundResponse
        {
            RefundId = refund.Id,
            OrderReference = order.SlugId,
            Amount = refund.Amount,
        };
    }
}