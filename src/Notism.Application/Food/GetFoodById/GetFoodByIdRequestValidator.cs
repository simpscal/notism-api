using FluentValidation;

namespace Notism.Application.Food.GetFoodById;

public class GetFoodByIdRequestValidator : AbstractValidator<GetFoodByIdRequest>
{
    public GetFoodByIdRequestValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Food ID is required");
    }
}