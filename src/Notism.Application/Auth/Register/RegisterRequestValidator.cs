using FluentValidation;

namespace Notism.Application.Auth.Register;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().NotNull().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().NotNull();
        RuleFor(x => x.LastName).NotEmpty().NotNull();
    }
}