using FluentValidation;

namespace Notism.Application.Order.AdminGetRevenueSeries;

public class AdminGetRevenueSeriesRequestValidator : AbstractValidator<AdminGetRevenueSeriesRequest>
{
    public AdminGetRevenueSeriesRequestValidator()
    {
        RuleFor(x => x.Boundaries)
            .NotNull()
            .Must(b => b is { Count: >= 2 })
            .WithMessage("At least 2 boundaries are required")
            .Must(BeStrictlyAscending)
            .WithMessage("Boundaries must be strictly ascending");

        RuleFor(x => x.Labels)
            .NotNull()
            .Must((request, labels) => labels is not null
                && request.Boundaries is not null
                && labels.Count == request.Boundaries.Count - 1)
            .WithMessage("Labels count must equal boundaries count minus one");
    }

    private static bool BeStrictlyAscending(List<DateTime>? boundaries)
    {
        if (boundaries is null || boundaries.Count < 2)
        {
            return true;
        }

        for (var i = 1; i < boundaries.Count; i++)
        {
            if (boundaries[i] <= boundaries[i - 1])
            {
                return false;
            }
        }

        return true;
    }
}