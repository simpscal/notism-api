using FluentValidation;

using Microsoft.Extensions.Localization;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;

namespace Notism.Application.Auth.Login;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).ValidatePassword(localizer);
    }
}