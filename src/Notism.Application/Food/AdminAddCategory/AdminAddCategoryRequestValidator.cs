using FluentValidation;

namespace Notism.Application.Food.AdminAddCategory;

public class AdminAddCategoryRequestValidator : AbstractValidator<AdminAddCategoryRequest>
{
    public AdminAddCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters");
    }
}
