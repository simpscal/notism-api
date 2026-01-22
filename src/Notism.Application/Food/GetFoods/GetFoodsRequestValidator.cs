using FluentValidation;

using Notism.Domain.Food.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.GetFoods;

public class GetFoodsRequestValidator : AbstractValidator<GetFoodsRequest>
{
    public GetFoodsRequestValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0");

        RuleFor(x => x.Take)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Take must be between 1 and 100");

        RuleFor(x => x.Category)
            .Must(category => category is null || category.ExistInEnum<FoodCategory>())
            .WithMessage("Invalid food category");
    }
}