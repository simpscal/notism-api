using FluentValidation;

using Notism.Domain.Food.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.AddFood;

public class AddFoodRequestValidator : AbstractValidator<AddFoodRequest>
{
    public AddFoodRequestValidator()
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
            .NotNull()
            .WithMessage("Price is required")
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero");

        RuleFor(x => x.Images)
            .NotEmpty()
            .WithMessage("At least one image is required");

        RuleForEach(x => x.Images)
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
            });

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required")
            .Must(category => category.ExistInEnum<FoodCategory>())
            .WithMessage("Invalid food category");

        RuleFor(x => x.QuantityUnit)
            .NotEmpty()
            .WithMessage("QuantityUnit is required")
            .Must(quantityUnit => quantityUnit.ExistInEnum<QuantityUnit>())
            .WithMessage("Invalid quantity unit");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("StockQuantity must be greater than or equal to zero");

        RuleFor(x => x.DiscountPrice)
            .Must((request, discountPrice) => !discountPrice.HasValue || discountPrice.Value < request.Price)
            .WithMessage("Discount price must be less than the original price")
            .Must((request, discountPrice) => !discountPrice.HasValue || discountPrice.Value > 0)
            .WithMessage("Discount price must be greater than zero");
    }
}