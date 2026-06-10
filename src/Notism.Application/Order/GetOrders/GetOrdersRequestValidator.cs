using FluentValidation;

using Notism.Application.Common.Services;
using Notism.Application.Common.Validators;

namespace Notism.Application.Order.GetOrders;

public class GetOrdersRequestValidator : AbstractValidator<GetOrdersRequest>
{
    public GetOrdersRequestValidator(IMessages messages)
    {
        RuleFor(x => x.Skip).ValidSkip(messages);
        RuleFor(x => x.Take).ValidTake(messages);
    }
}