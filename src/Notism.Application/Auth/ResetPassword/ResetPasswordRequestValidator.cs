using FluentValidation;

using Microsoft.Extensions.Localization;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;

namespace Notism.Application.Auth.ResetPassword;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .ValidatePassword(localizer);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match");
    }
}