using FluentValidation;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;

namespace Notism.Application.Auth.Login;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IMessages messages)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).ValidatePassword(messages);
    }
}