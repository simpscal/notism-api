using FluentValidation;

namespace Notism.Application.User.SaveBankAccount;

public class SaveBankAccountRequestValidator : AbstractValidator<SaveBankAccountRequest>
{
    public SaveBankAccountRequestValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("Owner ID is required");

        RuleFor(x => x.BankCode)
            .NotEmpty()
            .WithMessage("Bank name is required");

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account number is required");

        RuleFor(x => x.AccountHolderName)
            .NotEmpty()
            .WithMessage("Account holder name is required");
    }
}