using FluentAssertions;

using Notism.Shared.Utilities;

namespace Notism.Domain.Tests.Common;

public class DayWindowTests
{
    [Fact]
    public void HoChiMinhDay_AtLocalMidnight_MapsToPreviousDay17Utc()
    {
        // 2026-06-10T00:00:00 local (UTC+7) == 2026-06-09T17:00:00Z.
        var instant = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);

        var (startUtc, endUtc) = DayWindow.HoChiMinhDay(instant);

        startUtc.Should().Be(new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc));
        endUtc.Should().Be(new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void HoChiMinhDay_MiddayLocal_YieldsSameCivilDayWindow()
    {
        // 2026-06-10T12:30:00 local == 2026-06-10T05:30:00Z.
        var instant = new DateTime(2026, 6, 10, 5, 30, 0, DateTimeKind.Utc);

        var (startUtc, endUtc) = DayWindow.HoChiMinhDay(instant);

        startUtc.Should().Be(new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc));
        endUtc.Should().Be(new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void HoChiMinhDay_JustBeforeNextLocalMidnight_StaysInSameWindow()
    {
        // 2026-06-10T23:59:59 local == 2026-06-10T16:59:59Z, still inside the day.
        var instant = new DateTime(2026, 6, 10, 16, 59, 59, DateTimeKind.Utc);

        var (startUtc, endUtc) = DayWindow.HoChiMinhDay(instant);

        startUtc.Should().Be(new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc));
        endUtc.Should().Be(new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void HoChiMinhDay_NextLocalMidnight_RollsToNextWindow()
    {
        // 2026-06-11T00:00:00 local == 2026-06-10T17:00:00Z, the exclusive end of
        // the previous window becomes the inclusive start of the next.
        var instant = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc);

        var (startUtc, endUtc) = DayWindow.HoChiMinhDay(instant);

        startUtc.Should().Be(new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc));
        endUtc.Should().Be(new DateTime(2026, 6, 11, 17, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void HoChiMinhDay_WindowIsExactlyOneDayLong()
    {
        var instant = new DateTime(2026, 6, 10, 5, 30, 0, DateTimeKind.Utc);

        var (startUtc, endUtc) = DayWindow.HoChiMinhDay(instant);

        (endUtc - startUtc).Should().Be(TimeSpan.FromDays(1));
        startUtc.Kind.Should().Be(DateTimeKind.Utc);
        endUtc.Kind.Should().Be(DateTimeKind.Utc);
    }
}