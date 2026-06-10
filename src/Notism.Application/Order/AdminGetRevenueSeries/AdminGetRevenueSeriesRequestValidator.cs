using FluentValidation;

using Notism.Domain.Order.Repositories;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesRequestValidator : AbstractValidator<AdminGetRevenueSeriesRequest>
{
    public AdminGetRevenueSeriesRequestValidator()
    {
        RuleFor(x => x.Granularity)
            .NotEmpty()
            .WithMessage("Granularity is required")
            .Must(BeKnownGranularity)
            .WithMessage("Granularity must be 'year', 'month' or 'day'");
    }

    // FromCamelCase delegates to Enum.TryParse, which also accepts numeric strings
    // (e.g. "123") and bare member names. Restrict acceptance to the defined member
    // names so only year / month / day pass.
    private static bool BeKnownGranularity(string? granularity)
    {
        var parsed = granularity.FromCamelCase<RevenuePeriodGranularity>();
        return parsed != null && Enum.IsDefined(parsed.Value);
    }
}