using FluentValidation;

using Microsoft.Extensions.Localization;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsRequestValidator : AbstractValidator<GetFoodsRequest>
{
    public GetFoodsRequestValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Skip).ValidSkip(localizer);
        RuleFor(x => x.Take).ValidTake(localizer);

        RuleFor(x => x.SortOrder)
            .Must(sortOrder => sortOrder is null || sortOrder.FromCamelCase<SortOrder>() != null)
            .WithMessage("Sort order must be 'asc' or 'desc'");

        RuleFor(x => x.SortBy)
            .Must(sortBy => sortBy is null || IsValidSortBy(sortBy))
            .WithMessage("Sort by must be 'name', 'price', 'discountPrice', or 'createdAt'");
    }

    private static bool IsValidSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return true;
        }

        return sortBy is "name" or "price" or "discountPrice" or "createdAt";
    }
}