using FluentValidation;

namespace Notism.Application.Food.AdminUpdateCustomisationOption;

public class AdminUpdateCustomisationOptionRequestValidator : AbstractValidator<AdminUpdateCustomisationOptionRequest>
{
    public AdminUpdateCustomisationOptionRequestValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Label cannot be empty when provided")
            .MaximumLength(100)
            .WithMessage("Label cannot exceed 100 characters")
            .When(x => x.Label != null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder must be greater than or equal to zero")
            .When(x => x.DisplayOrder.HasValue);

        RuleFor(x => x.Surcharge)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Surcharge must be greater than or equal to zero")
            .When(x => x.Surcharge.HasValue);
    }
}
