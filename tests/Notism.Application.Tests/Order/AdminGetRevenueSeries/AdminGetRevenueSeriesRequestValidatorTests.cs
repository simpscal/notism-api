using FluentAssertions;

using Notism.Application.Order.AdminGetRevenueSeries;

namespace Notism.Application.Tests.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesRequestValidatorTests
{
    private readonly AdminGetRevenueSeriesRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenBoundariesAscendingAndLabelsMatch_Passes()
    {
        var request = new AdminGetRevenueSeriesRequest
        {
            Boundaries = Boundaries(3),
            Labels = new List<string> { "a", "b" },
            Granularity = "day",
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenSingleBucketTwoBoundaries_Passes()
    {
        var request = new AdminGetRevenueSeriesRequest
        {
            Boundaries = Boundaries(2),
            Labels = new List<string> { "only" },
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenFewerThanTwoBoundaries_Fails()
    {
        var request = new AdminGetRevenueSeriesRequest
        {
            Boundaries = Boundaries(1),
            Labels = new List<string>(),
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetRevenueSeriesRequest.Boundaries));
    }

    [Fact]
    public async Task Validate_WhenBoundariesNotStrictlyAscending_Fails()
    {
        var instant = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var request = new AdminGetRevenueSeriesRequest
        {
            // Duplicate boundary -> not strictly ascending.
            Boundaries = new List<DateTime> { instant, instant, instant.AddDays(1) },
            Labels = new List<string> { "a", "b" },
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetRevenueSeriesRequest.Boundaries));
    }

    [Fact]
    public async Task Validate_WhenBoundariesDescending_Fails()
    {
        var start = new DateTime(2026, 6, 3, 0, 0, 0, DateTimeKind.Utc);
        var request = new AdminGetRevenueSeriesRequest
        {
            Boundaries = new List<DateTime> { start, start.AddDays(-1), start.AddDays(-2) },
            Labels = new List<string> { "a", "b" },
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetRevenueSeriesRequest.Boundaries));
    }

    [Fact]
    public async Task Validate_WhenLabelCountDoesNotMatchBucketCount_Fails()
    {
        var request = new AdminGetRevenueSeriesRequest
        {
            Boundaries = Boundaries(3),
            Labels = new List<string> { "a" },
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetRevenueSeriesRequest.Labels));
    }

    [Fact]
    public async Task Validate_WhenGranularityOmitted_Passes()
    {
        var request = new AdminGetRevenueSeriesRequest
        {
            Boundaries = Boundaries(2),
            Labels = new List<string> { "only" },
            Granularity = null,
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    private static List<DateTime> Boundaries(int count)
    {
        var start = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        return Enumerable.Range(0, count)
            .Select(i => start.AddDays(i))
            .ToList();
    }
}