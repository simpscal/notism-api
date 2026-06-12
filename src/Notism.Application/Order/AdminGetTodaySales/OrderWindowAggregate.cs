namespace Notism.Application.Order.AdminGetTodaySales;

/// <summary>
/// Server-side aggregates over a [startUtc, endUtc) window.
/// <para><see cref="Revenue"/> sums <c>TotalAmount</c> of Paid orders whose
/// <c>PaidAt</c> falls in the window.</para>
/// <para><see cref="OrderCount"/> counts orders whose <c>CreatedAt</c> falls in the
/// window. These predicates are intentionally distinct.</para>
/// </summary>
public sealed record OrderWindowAggregate(decimal Revenue, int OrderCount);