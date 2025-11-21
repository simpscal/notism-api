using FluentValidation;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthCallbackRequestValidator : AbstractValidator<GoogleOAuthCallbackRequest>
{
    public GoogleOAuthCallbackRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Authorization code is required");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State parameter is required for security validation");
    }
}

