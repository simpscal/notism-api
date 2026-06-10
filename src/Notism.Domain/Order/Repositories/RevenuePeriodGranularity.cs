namespace Notism.Domain.Order.Repositories;

/// <summary>
/// Civil-period granularity for the period-bucketed revenue aggregate. Periods
/// are evaluated in Asia/Ho_Chi_Minh (UTC+7) civil time, consistent with
/// <see cref="Notism.Shared.Utilities.DayWindow"/>.
/// </summary>
public enum RevenuePeriodGranularity
{
    Day = 0,
    Month = 1,
    Year = 2,
}