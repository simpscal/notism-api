using FluentValidation;

namespace Notism.Application.Food.AdminAddCustomisationOption;

public class AdminAddCustomisationOptionRequestValidator : AbstractValidator<AdminAddCustomisationOptionRequest>
{
    public AdminAddCustomisationOptionRequestValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Label is required")
            .MaximumLength(100)
            .WithMessage("Label cannot exceed 100 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder must be greater than or equal to zero");

        RuleFor(x => x.Surcharge)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Surcharge must be greater than or equal to zero")
            .When(x => x.Surcharge.HasValue);
    }
}
