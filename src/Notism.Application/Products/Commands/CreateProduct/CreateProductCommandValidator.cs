using FluentValidation;

using Notism.Domain.Common.Enums;
using Notism.Shared.Extensions;

using Throw;

namespace Notism.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Category)
            .Must(category => category.ExistInEnum<CategoryType>())
            .WithMessage("Category does not exist");

        RuleFor(x => x.SubCategory)
            .Must(subCategory => subCategory.ExistInEnum<SubCategoryType>())
            .WithMessage("Sub Category does not exist");

        RuleFor(x => x.Colors).Must(colors => colors.Length > 0)
            .WithMessage("Colors are empty")
            .Must(colors => colors.All(color => color.ExistInEnum<ColorType>()))
            .WithMessage("Colors do not exist");

        RuleFor(x => x.Sizes).Must(sizes => sizes.Length > 0)
            .WithMessage("Sizes are empty")
            .Must(sizes => sizes.All(size => size.ExistInEnum<SizeType>()))
            .WithMessage("Sizes do not exist");
    }
}