using FluentValidation;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;
using Notism.Domain.Order.Enums;
using Notism.Shared.Extensions;

namespace Notism.Application.Order.AdminRefundsForTable;

public class AdminRefundsForTableRequestValidator : AbstractValidator<AdminRefundsForTableRequest>
{
    public AdminRefundsForTableRequestValidator(IMessages messages)
    {
        RuleFor(x => x.Skip).ValidSkip(messages);
        RuleFor(x => x.Take).ValidTake(messages);

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || status.ExistInEnum<RefundStatus>())
            .WithMessage("Status must be 'pending', 'processing', 'paid' or 'failed'");
    }
}