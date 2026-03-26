using FluentValidation;

using Microsoft.Extensions.Localization;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;

namespace Notism.Application.Auth.Register;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).ValidatePassword(localizer);
        RuleFor(x => x.FirstName).NotEmpty().NotNull();
        RuleFor(x => x.LastName).NotEmpty().NotNull();
    }
}