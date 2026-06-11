namespace Notism.Domain.Order.Repositories;

/// <summary>
/// Order counts folded into the dashboard delivery-status taxonomy.
/// Mapping (server-side): New = OrderPlaced; InProgress = Preparing + OnTheWay;
/// Completed = Delivered. Cancelled orders are excluded from every bucket.
/// Every bucket is always present, defaulting to zero when no orders match.
/// </summary>
public sealed record OrderStatusBucketCounts(int New, int InProgress, int Completed);