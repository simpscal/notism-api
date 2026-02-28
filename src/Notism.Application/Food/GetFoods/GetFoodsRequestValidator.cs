using FluentValidation;

using Notism.Application.Common.Validators;
using Notism.Domain.Food.Enums;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsRequestValidator : AbstractValidator<GetFoodsRequest>
{
    public GetFoodsRequestValidator()
    {
        RuleFor(x => x.Skip).ValidSkip();
        RuleFor(x => x.Take).ValidTake();

        RuleFor(x => x.Category)
            .ValidOptionalEnum<GetFoodsRequest, FoodCategory>("Category");

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