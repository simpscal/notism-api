using FluentValidation;

namespace Notism.Application.Food.AdminAddCustomisationGroup;

public class AdminAddCustomisationGroupRequestValidator : AbstractValidator<AdminAddCustomisationGroupRequest>
{
    public AdminAddCustomisationGroupRequestValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .WithMessage("Label is required")
            .MaximumLength(100)
            .WithMessage("Label cannot exceed 100 characters");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("DisplayOrder must be greater than or equal to zero");
    }
}