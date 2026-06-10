using FluentValidation;

namespace Notism.Application.Order.AdminGetTodaySales;

public class AdminGetTodaySalesRequestValidator : AbstractValidator<AdminGetTodaySalesRequest>
{
    public AdminGetTodaySalesRequestValidator()
    {
        // The client supplies both UTC boundaries; the server derives no window.
        RuleFor(x => x.StartUtc)
            .NotEmpty()
            .WithMessage("StartUtc is required");

        RuleFor(x => x.EndUtc)
            .NotEmpty()
            .WithMessage("EndUtc is required")
            .GreaterThan(x => x.StartUtc)
            .WithMessage("EndUtc must be after StartUtc");
    }
}