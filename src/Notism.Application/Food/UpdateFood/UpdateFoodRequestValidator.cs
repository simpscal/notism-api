using FluentValidation;

using Notism.Domain.Food.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Food.UpdateFood;

public class UpdateFoodRequestValidator : AbstractValidator<UpdateFoodRequest>
{
    public UpdateFoodRequestValidator()
    {
        RuleFor(x => x.FoodId)
            .NotEmpty()
            .WithMessage("Food ID is required");

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero")
            .When(x => x.Price.HasValue);

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
            .When(x => x.Images != null && x.Images.Any());

        RuleFor(x => x.Category)
            .Must(category => category != null && category.ExistInEnum<FoodCategory>())
            .WithMessage("Invalid food category")
            .When(x => !string.IsNullOrWhiteSpace(x.Category));

        RuleFor(x => x.QuantityUnit)
            .Must(quantityUnit => quantityUnit != null && quantityUnit.ExistInEnum<QuantityUnit>())
            .WithMessage("Invalid quantity unit")
            .When(x => !string.IsNullOrWhiteSpace(x.QuantityUnit));

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("StockQuantity must be greater than or equal to zero")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.DiscountPrice)
            .Must((request, discountPrice) => !discountPrice.HasValue || (request.Price.HasValue && discountPrice.Value < request.Price.Value))
            .WithMessage("Discount price must be less than the original price")
            .When(x => x.DiscountPrice.HasValue && x.Price.HasValue)
            .Must((request, discountPrice) => !discountPrice.HasValue || discountPrice.Value > 0)
            .WithMessage("Discount price must be greater than zero")
            .When(x => x.DiscountPrice.HasValue);
    }
}

