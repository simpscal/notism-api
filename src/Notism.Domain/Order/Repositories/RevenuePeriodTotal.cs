namespace Notism.Domain.Order.Repositories;

/// <summary>
/// A single GROUP BY row of the period-bucketed revenue aggregate.
/// <para><see cref="PeriodStartUtc"/> is the inclusive UTC start of the
/// Asia/Ho_Chi_Minh (UTC+7) civil period (year / month / day) the bucket
/// represents.</para>
/// <para><see cref="Revenue"/> sums <c>TotalAmount</c> of Paid orders whose
/// <c>PaidAt</c> falls in that period.</para>
/// Only periods that contain Paid orders are produced; the consuming handler
/// owns zero-filling missing periods.
/// </summary>
public sealed record RevenuePeriodTotal(DateTime PeriodStartUtc, decimal Revenue);