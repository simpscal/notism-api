namespace Notism.Application.Order.AdminGetRevenueSeries;

/// <summary>
/// A single GROUP BY row of the bucketed revenue aggregate.
/// <para><see cref="BucketIndex"/> is the zero-based index of the half-open
/// <c>[boundaries[i], boundaries[i+1])</c> UTC range the bucket represents.</para>
/// <para><see cref="Revenue"/> sums <c>TotalAmount</c> (full <c>numeric</c> precision)
/// of Paid orders whose <c>PaidAt</c> falls in that range.</para>
/// Only buckets that contain Paid orders are produced; the consuming handler owns
/// zero-filling absent indices into a dense ordered series.
/// </summary>
public sealed record RevenueBucketTotal(int BucketIndex, decimal Revenue);
