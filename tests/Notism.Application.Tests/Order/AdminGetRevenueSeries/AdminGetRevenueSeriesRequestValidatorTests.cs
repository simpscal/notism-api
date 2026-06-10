using FluentAssertions;

using Notism.Application.Order.AdminGetRevenueSeries;

namespace Notism.Application.Tests.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesRequestValidatorTests
{
    private readonly AdminGetRevenueSeriesRequestValidator _validator = new();

    [Theory]
    [InlineData("year")]
    [InlineData("month")]
    [InlineData("day")]
    [InlineData("Year")]
    [InlineData("DAY")]
    public async Task Validate_WhenGranularityIsKnown_Passes(string granularity)
    {
        var request = new AdminGetRevenueSeriesRequest { Granularity = granularity };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("week")]
    [InlineData("quarter")]
    [InlineData("123")]
    public async Task Validate_WhenGranularityIsMissingOrInvalid_Fails(string? granularity)
    {
        var request = new AdminGetRevenueSeriesRequest { Granularity = granularity };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetRevenueSeriesRequest.Granularity));
    }
}