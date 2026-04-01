using FluentValidation;

namespace Notism.Application.Food.AdminUpdateCategory;

public class AdminUpdateCategoryRequestValidator : AbstractValidator<AdminUpdateCategoryRequest>
{
    public AdminUpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters");
    }
}