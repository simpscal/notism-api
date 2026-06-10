namespace Notism.Shared.Utilities;

/// <summary>
/// Computes civil-day [startUtc, endUtc) windows for a fixed time zone.
/// Used by dashboard aggregates so that "today" reflects the local business
/// day rather than the UTC day.
/// </summary>
public static class DayWindow
{
    /// <summary>
    /// Asia/Ho_Chi_Minh is a fixed UTC+7 offset with no daylight saving time,
    /// so a plain offset computation is sufficient and deterministic.
    /// </summary>
    public static readonly TimeSpan HoChiMinhOffset = TimeSpan.FromHours(7);

    /// <summary>
    /// Returns the half-open [startUtc, endUtc) UTC window covering the
    /// Asia/Ho_Chi_Minh (UTC+7) civil day that contains the supplied instant.
    /// </summary>
    /// <param name="instantUtc">An instant. Treated as UTC.</param>
    /// <returns>A tuple of the inclusive start and exclusive end of the day, both in UTC.</returns>
    public static (DateTime StartUtc, DateTime EndUtc) HoChiMinhDay(DateTime instantUtc)
    {
        var utc = DateTime.SpecifyKind(instantUtc, DateTimeKind.Utc);

        // Shift into local civil time, truncate to local midnight, then shift back to UTC.
        var local = utc + HoChiMinhOffset;
        var localMidnight = local.Date;

        var startUtc = DateTime.SpecifyKind(localMidnight - HoChiMinhOffset, DateTimeKind.Utc);
        var endUtc = startUtc.AddDays(1);

        return (startUtc, endUtc);
    }
}