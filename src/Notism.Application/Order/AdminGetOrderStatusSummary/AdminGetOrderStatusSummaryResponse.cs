namespace Notism.Application.Order.AdminGetOrderStatusSummary;

public sealed record AdminGetOrderStatusSummaryResponse
{
    public int New { get; init; }
    public int InProgress { get; init; }
    public int Completed { get; init; }

    public static AdminGetOrderStatusSummaryResponse FromDomain(OrderStatusBucketCounts counts)
    {
        return new AdminGetOrderStatusSummaryResponse
        {
            New = counts.New,
            InProgress = counts.InProgress,
            Completed = counts.Completed,
        };
    }
}