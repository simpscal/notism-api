using FluentValidation;

using Microsoft.Extensions.Localization;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;

namespace Notism.Application.Order.GetOrders;

public class GetOrdersRequestValidator : AbstractValidator<GetOrdersRequest>
{
    public GetOrdersRequestValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Skip).ValidSkip(localizer);
        RuleFor(x => x.Take).ValidTake(localizer);
    }
}