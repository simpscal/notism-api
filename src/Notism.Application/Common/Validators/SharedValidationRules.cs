using FluentValidation;

using Notism.Application.Common.Services;
using Notism.Shared.Extensions;

namespace Notism.Application.Common.Validators;

public static class SharedValidationRules
{
    private const int DefaultMaxTake = 100;

    public static IRuleBuilderOptions<T, int> ValidSkip<T>(this IRuleBuilder<T, int> ruleBuilder, IMessages messages)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithMessage(_ => messages.SkipMustBeNonNegative);
    }

    public static IRuleBuilderOptions<T, int> ValidTake<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        IMessages messages,
        int maxTake = DefaultMaxTake)
    {
        return ruleBuilder
            .GreaterThan(0)
            .LessThanOrEqualTo(maxTake)
            .WithMessage(_ => string.Format(messages.TakeMustBeBetween, maxTake));
    }

    public static IRuleBuilderOptions<T, string?> ValidRequiredEnum<T, TEnum>(
        this IRuleBuilder<T, string?> ruleBuilder,
        IMessages messages,
        string propertyDisplayName = "Value")
        where TEnum : Enum
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(_ => string.Format(messages.FieldRequired, propertyDisplayName))
            .Must(value => value!.ExistInEnum<TEnum>())
            .WithMessage(_ => string.Format(messages.InvalidValue, propertyDisplayName.ToLowerInvariant()));
    }

    public static IRuleBuilderOptions<T, string?> ValidOptionalEnum<T, TEnum>(
        this IRuleBuilder<T, string?> ruleBuilder,
        IMessages messages,
        string propertyDisplayName = "Value")
        where TEnum : Enum
    {
        return ruleBuilder
            .Must(value => string.IsNullOrWhiteSpace(value) || value.ExistInEnum<TEnum>())
            .WithMessage(_ => string.Format(messages.InvalidValue, propertyDisplayName.ToLowerInvariant()));
    }
}