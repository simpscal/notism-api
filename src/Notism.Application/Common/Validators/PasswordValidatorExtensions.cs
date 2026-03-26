using System.Text.RegularExpressions;

using FluentValidation;

using Microsoft.Extensions.Localization;

using Notism.Application.Common.Services;

namespace Notism.Application.Common.Validators;

public static class PasswordValidatorExtensions
{
    private const string SpecialSymbolsPattern = @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]";
    private const string UppercaseLetterPattern = @"[A-Z]";
    private const string NumberPattern = @"[0-9]";
    private const int MinimumPasswordLength = 8;

    public static IRuleBuilderOptions<T, string> ValidatePassword<T>(this IRuleBuilder<T, string> ruleBuilder, IStringLocalizer<Messages> localizer)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(_ => localizer["PasswordRequired"])
            .NotNull()
            .WithMessage(_ => localizer["PasswordRequired"])
            .MinimumLength(MinimumPasswordLength)
            .WithMessage(_ => localizer["PasswordMinLength"])
            .Must(HaveSpecialSymbol)
            .WithMessage(_ => localizer["PasswordSpecialChar"])
            .Must(HaveCapitalizedLetter)
            .WithMessage(_ => localizer["PasswordUppercase"])
            .Must(HaveNumber)
            .WithMessage(_ => localizer["PasswordNumber"]);
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