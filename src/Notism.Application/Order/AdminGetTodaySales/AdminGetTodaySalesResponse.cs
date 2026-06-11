namespace Notism.Application.Order.AdminGetTodaySales;

public sealed record AdminGetTodaySalesResponse
{
    public decimal Revenue { get; init; }
    public int OrderCount { get; init; }

    public static AdminGetTodaySalesResponse FromDomain(OrderWindowAggregate aggregate)
    {
        return new AdminGetTodaySalesResponse
        {
            Revenue = aggregate.Revenue,
            OrderCount = aggregate.OrderCount,
        };
    }
}