using FluentValidation;

namespace Notism.Application.User.AdminDeleteUser;

public class AdminDeleteUserRequestValidator : AbstractValidator<AdminDeleteUserRequest>
{
    public AdminDeleteUserRequestValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}