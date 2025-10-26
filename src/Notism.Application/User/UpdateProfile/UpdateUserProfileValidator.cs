using FluentValidation;

using Notism.Application.Common.Utilities;
using Notism.Domain.User.Enums;

namespace Notism.Application.User.UpdateProfile;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Email must be a valid email address");

        RuleFor(x => x.Role)
            .Must(role => EnumConverter.IsValidEnumString<UserRole>(role))
            .WithMessage($"Role must be either '{string.Join("' or '", EnumConverter.GetValidEnumStrings<UserRole>())}'");
    }
}