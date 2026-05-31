using FluentValidation;

namespace Notism.Application.Food.AdminUpdateCustomisationGroup;

public class AdminUpdateCustomisationGroupRequestValidator : AbstractValidator<AdminUpdateCustomisationGroupRequest>
{
    public AdminUpdateCustomisationGroupRequestValidator()
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
    }
}
