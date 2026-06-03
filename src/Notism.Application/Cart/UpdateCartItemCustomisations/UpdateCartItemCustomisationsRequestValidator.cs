using FluentValidation;

namespace Notism.Application.Cart.UpdateCartItemCustomisations;

public class UpdateCartItemCustomisationsRequestValidator : AbstractValidator<UpdateCartItemCustomisationsRequest>
{
    public UpdateCartItemCustomisationsRequestValidator()
    {
        RuleFor(x => x.CartItemId)
            .NotEmpty()
            .WithMessage("CartItemId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Customisations)
            .NotNull()
            .WithMessage("Customisations is required");

        RuleForEach(x => x.Customisations).ChildRules(selection =>
        {
            selection.RuleFor(s => s.GroupId)
                .NotEmpty()
                .WithMessage("GroupId is required");

            selection.RuleFor(s => s.OptionId)
                .NotEmpty()
                .WithMessage("OptionId is required");
        });
    }
}
