using FluentAssertions;

using Notism.Application.Order.AdminGetTodaySales;

namespace Notism.Application.Tests.Order.AdminGetTodaySales;

public class AdminGetTodaySalesRequestValidatorTests
{
    private readonly AdminGetTodaySalesRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenStartBeforeEnd_Passes()
    {
        var request = new AdminGetTodaySalesRequest
        {
            StartUtc = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc),
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenStartUtcMissing_Fails()
    {
        var request = new AdminGetTodaySalesRequest
        {
            EndUtc = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc),
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetTodaySalesRequest.StartUtc));
    }

    [Fact]
    public async Task Validate_WhenEndUtcMissing_Fails()
    {
        var request = new AdminGetTodaySalesRequest
        {
            StartUtc = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc),
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetTodaySalesRequest.EndUtc));
    }

    [Fact]
    public async Task Validate_WhenStartEqualsEnd_Fails()
    {
        var instant = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc);
        var request = new AdminGetTodaySalesRequest { StartUtc = instant, EndUtc = instant };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetTodaySalesRequest.EndUtc));
    }

    [Fact]
    public async Task Validate_WhenStartAfterEnd_Fails()
    {
        var request = new AdminGetTodaySalesRequest
        {
            StartUtc = new DateTime(2026, 6, 10, 17, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc),
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminGetTodaySalesRequest.EndUtc));
    }
}