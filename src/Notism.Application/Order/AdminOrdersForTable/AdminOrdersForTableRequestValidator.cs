using FluentValidation;

using Notism.Application.Common.Validators;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminOrdersForTable;

public class AdminOrdersForTableRequestValidator : AbstractValidator<AdminOrdersForTableRequest>
{
    public AdminOrdersForTableRequestValidator()
    {
        RuleFor(x => x.Skip).ValidSkip();
        RuleFor(x => x.Take).ValidTake();

        RuleFor(x => x.SortOrder)
            .Must(sortOrder => sortOrder is null || sortOrder.FromCamelCase<SortOrder>() != null)
            .WithMessage("Sort order must be 'asc' or 'desc'");

        RuleFor(x => x.SortBy)
            .Must(sortBy => sortBy is null || IsValidSortBy(sortBy))
            .WithMessage("Sort by must be 'slugId' or 'totalAmount'");
    }

    private static bool IsValidSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return true;
        }

        var sortByLower = sortBy.ToLower();
        return sortByLower == "slugId" || sortByLower == "totalAmount";
    }
}