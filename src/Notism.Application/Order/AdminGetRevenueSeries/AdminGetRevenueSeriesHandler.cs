using System.Globalization;

using MediatR;

using Microsoft.Extensions.Logging;

using Notism.Domain.Order.Repositories;
using Notism.Shared.Extensions;
using Notism.Shared.Utilities;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesHandler
    : IRequestHandler<AdminGetRevenueSeriesRequest, AdminGetRevenueSeriesResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<AdminGetRevenueSeriesHandler> _logger;

    public AdminGetRevenueSeriesHandler(
        IOrderRepository orderRepository,
        ILogger<AdminGetRevenueSeriesHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<AdminGetRevenueSeriesResponse> Handle(
        AdminGetRevenueSeriesRequest request,
        CancellationToken cancellationToken)
    {
        // The validator guarantees a parseable granularity; default keeps the
        // type non-nullable for the switch below.
        var granularity = request.Granularity.FromCamelCase<RevenuePeriodGranularity>()
                          ?? RevenuePeriodGranularity.Day;

        var populated = await _orderRepository.GetRevenueByPeriodAsync(granularity);

        // Index populated periods by their UTC+7 civil period label so the dense
        // range can be left-joined (0 for gaps). PeriodStartUtc is the inclusive
        // UTC start of the civil period; shift back into UTC+7 to read its label.
        var revenueByPeriod = populated.ToDictionary(
            p => PeriodLabel(granularity, ToLocal(p.PeriodStartUtc)),
            p => p.Revenue);

        // "Now" in UTC+7 civil time drives the server-derived range.
        var nowLocal = ToLocal(DateTime.UtcNow);

        var points = BuildDensePoints(granularity, nowLocal, populated, revenueByPeriod);

        _logger.LogInformation(
            "Retrieved revenue series for granularity {Granularity}: {PopulatedCount} populated period(s), {PointCount} dense point(s)",
            granularity,
            populated.Count,
            points.Count);

        return AdminGetRevenueSeriesResponse.FromPoints(granularity, points);
    }

    private static IReadOnlyList<RevenueSeriesPoint> BuildDensePoints(
        RevenuePeriodGranularity granularity,
        DateTime nowLocal,
        IReadOnlyList<RevenuePeriodTotal> populated,
        IReadOnlyDictionary<string, decimal> revenueByPeriod)
    {
        var labels = granularity switch
        {
            RevenuePeriodGranularity.Year => YearLabels(nowLocal, populated),
            RevenuePeriodGranularity.Month => MonthLabels(nowLocal),
            RevenuePeriodGranularity.Day => DayLabels(nowLocal),
            _ => DayLabels(nowLocal),
        };

        return labels
            .Select(label => new RevenueSeriesPoint
            {
                Period = label,
                Revenue = revenueByPeriod.TryGetValue(label, out var revenue) ? revenue : 0m,
            })
            .ToList();
    }

    // Year: earliest paid order's UTC+7 year through the current year. With zero
    // paid orders the range collapses to the current year's single point.
    private static IEnumerable<string> YearLabels(
        DateTime nowLocal,
        IReadOnlyList<RevenuePeriodTotal> populated)
    {
        var currentYear = nowLocal.Year;
        var earliestYear = populated.Count == 0
            ? currentYear
            : populated.Min(p => ToLocal(p.PeriodStartUtc).Year);

        // Guard against any stray future-dated period leaking past the current year.
        if (earliestYear > currentYear)
        {
            earliestYear = currentYear;
        }

        for (var year = earliestYear; year <= currentYear; year++)
        {
            yield return year.ToString(CultureInfo.InvariantCulture);
        }
    }

    // Month: the 12 months of the current UTC+7 year.
    private static IEnumerable<string> MonthLabels(DateTime nowLocal)
    {
        for (var month = 1; month <= 12; month++)
        {
            yield return PeriodLabel(
                RevenuePeriodGranularity.Month,
                new DateTime(nowLocal.Year, month, 1));
        }
    }

    // Day: every day of the current UTC+7 month.
    private static IEnumerable<string> DayLabels(DateTime nowLocal)
    {
        var daysInMonth = DateTime.DaysInMonth(nowLocal.Year, nowLocal.Month);
        for (var day = 1; day <= daysInMonth; day++)
        {
            yield return PeriodLabel(
                RevenuePeriodGranularity.Day,
                new DateTime(nowLocal.Year, nowLocal.Month, day));
        }
    }

    private static string PeriodLabel(RevenuePeriodGranularity granularity, DateTime local)
    {
        return granularity switch
        {
            RevenuePeriodGranularity.Year => local.ToString("yyyy", CultureInfo.InvariantCulture),
            RevenuePeriodGranularity.Month => local.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            RevenuePeriodGranularity.Day => local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        };
    }

    // Shift a UTC instant into Asia/Ho_Chi_Minh (UTC+7) civil time, matching the
    // fixed-offset convention used by DayWindow and the repository aggregate.
    private static DateTime ToLocal(DateTime instantUtc)
    {
        return DateTime.SpecifyKind(instantUtc, DateTimeKind.Utc) + DayWindow.HoChiMinhOffset;
    }
}