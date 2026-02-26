using FluentValidation;

using Notism.Application.Common.Validators;
using Notism.Domain.Food.Enums;

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

        RuleFor(x => x.Category)
            .ValidOptionalEnum<UpdateFoodRequest, FoodCategory>("Category");

        RuleFor(x => x.QuantityUnit)
            .ValidOptionalEnum<UpdateFoodRequest, QuantityUnit>("QuantityUnit");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("StockQuantity must be greater than or equal to zero")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.DiscountPrice)
            .Must((request, discountPrice) => !discountPrice.HasValue || (request.Price.HasValue && discountPrice.Value < request.Price.Value))
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
