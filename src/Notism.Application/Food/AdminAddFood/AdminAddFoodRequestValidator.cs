using FluentValidation;

using Notism.Application.Common.Validators;
using Notism.Domain.Food.Enums;

namespace Notism.Application.Food.AdminAddFood;

public class AdminAddFoodRequestValidator : AbstractValidator<AdminAddFoodRequest>
{
    public AdminAddFoodRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required")
            .MaximumLength(200)
            .WithMessage("Category cannot exceed 200 characters");

        RuleFor(x => x.QuantityUnit)
            .ValidRequiredEnum<AdminAddFoodRequest, QuantityUnit>("QuantityUnit");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("StockQuantity must be greater than or equal to zero");

        RuleFor(x => x.DiscountPrice)
            .Must((request, discountPrice) => !discountPrice.HasValue || discountPrice.Value < request.Price)
            .WithMessage("Discount price must be less than the original price");

        RuleForEach(x => x.Images!)
            .ChildRules(image =>
            {
                image.RuleFor(i => i.FileKey)
                    .NotEmpty()
                    .WithMessage("FileKey is required for each image")
                    .MaximumLength(500)
                    .WithMessage("FileKey cannot exceed 500 characters");

                image.RuleFor(i => i.DisplayOrder)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("DisplayOrder must be greater than or equal to zero");
            })
            .When(x => x.Images != null && x.Images.Count > 0);
    }
}
