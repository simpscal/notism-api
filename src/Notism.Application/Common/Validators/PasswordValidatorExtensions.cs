using System.Text.RegularExpressions;

using FluentValidation;

namespace Notism.Application.Common.Validators;

public static class PasswordValidatorExtensions
{
    private const string SpecialSymbolsPattern = @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]";
    private const string UppercaseLetterPattern = @"[A-Z]";
    private const string NumberPattern = @"[0-9]";
    private const int MinimumPasswordLength = 8;

    public static IRuleBuilderOptions<T, string> ValidatePassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Password is required")
            .NotNull()
            .WithMessage("Password is required")
            .MinimumLength(MinimumPasswordLength)
            .WithMessage($"Password must be at least {MinimumPasswordLength} characters long")
            .Must(HaveSpecialSymbol)
            .WithMessage("Password must contain at least one special symbol")
            .Must(HaveCapitalizedLetter)
            .WithMessage("Password must contain at least one uppercase letter")
            .Must(HaveNumber)
            .WithMessage("Password must contain at least one number");
    }

    private static bool HaveSpecialSymbol(string? password)
    {
        return string.IsNullOrWhiteSpace(password) ? false : Regex.IsMatch(password, SpecialSymbolsPattern);
    }

    private static bool HaveCapitalizedLetter(string? password)
    {
        return string.IsNullOrWhiteSpace(password) ? false : Regex.IsMatch(password, UppercaseLetterPattern);
    }

    private static bool HaveNumber(string? password)
    {
        return string.IsNullOrWhiteSpace(password) ? false : Regex.IsMatch(password, NumberPattern);
    }
}