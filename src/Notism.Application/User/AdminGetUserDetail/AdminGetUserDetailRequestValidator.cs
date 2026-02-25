using FluentValidation;

namespace Notism.Application.User.AdminGetUserDetail;

public class AdminGetUserDetailRequestValidator : AbstractValidator<AdminGetUserDetailRequest>
{
    public AdminGetUserDetailRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
