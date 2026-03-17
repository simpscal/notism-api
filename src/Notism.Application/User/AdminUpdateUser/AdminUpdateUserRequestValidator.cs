using FluentValidation;

using Notism.Domain.User.Enums;

namespace Notism.Application.User.AdminUpdateUser;

public class AdminUpdateUserRequestValidator : AbstractValidator<AdminUpdateUserRequest>
{
    public AdminUpdateUserRequestValidator()
    {
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(BeValidRole)
            .WithMessage("Role must be 'user' or 'admin'.");
    }

    private static bool BeValidRole(string role) =>
        !string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out _);
}