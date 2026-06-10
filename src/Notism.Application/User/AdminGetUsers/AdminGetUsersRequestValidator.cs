using FluentValidation;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;
using Notism.Shared.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.User.AdminGetUsers;

public class AdminGetUsersRequestValidator : AbstractValidator<AdminGetUsersRequest>
{
    public AdminGetUsersRequestValidator(IMessages messages)
    {
        RuleFor(x => x.Skip).ValidSkip(messages);
        RuleFor(x => x.Take).ValidTake(messages);

        RuleFor(x => x.SortOrder)
            .Must(sortOrder => sortOrder is null || sortOrder.FromCamelCase<SortOrder>() != null)
            .WithMessage("Sort order must be 'asc' or 'desc'");

        RuleFor(x => x.SortBy)
            .Must(sortBy => sortBy is null || IsValidSortBy(sortBy))
            .WithMessage("Sort by must be 'firstName', 'lastName', 'email', or 'role'");
    }

    private static bool IsValidSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return true;
        }

        return sortBy == "firstName" ||
               sortBy == "lastName" ||
               sortBy == "email" ||
               sortBy == "role";
    }
}