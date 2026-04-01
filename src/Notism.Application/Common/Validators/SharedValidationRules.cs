using FluentValidation;

using Microsoft.Extensions.Localization;

using Notism.Application.Common.Services;
using Notism.Shared.Extensions;

namespace Notism.Application.Common.Validators;

/// <summary>
/// Reusable FluentValidation rule extensions for pagination (Skip/Take) and enum/category validation.
/// </summary>
public static class SharedValidationRules
{
    private const int DefaultMaxTake = 100;

    /// <summary>
    /// Validates that Skip is greater than or equal to 0.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder.</param>
    /// <returns>The rule builder options.</returns>
    public static IRuleBuilderOptions<T, int> ValidSkip<T>(this IRuleBuilder<T, int> ruleBuilder, IStringLocalizer<Messages> localizer)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0)
            .WithMessage(_ => localizer["SkipMustBeNonNegative"]);
    }

    /// <summary>
    /// Validates that Take is between 1 and maxTake (default 100).
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder.</param>
    /// <param name="maxTake">Maximum allowed value for Take (default 100).</param>
    /// <returns>The rule builder options.</returns>
    public static IRuleBuilderOptions<T, int> ValidTake<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        IStringLocalizer<Messages> localizer,
        int maxTake = DefaultMaxTake)
    {
        return ruleBuilder
            .GreaterThan(0)
            .LessThanOrEqualTo(maxTake)
            .WithMessage(_ => string.Format(localizer["TakeMustBeBetween"], maxTake));
    }

    /// <summary>
    /// Validates that a required string value is a valid enum value (e.g. category).
    /// Use for non-nullable or required properties.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TEnum">The enum type to validate against.</typeparam>
    /// <param name="ruleBuilder">The rule builder.</param>
    /// <param name="propertyDisplayName">Display name for the property in error messages.</param>
    /// <returns>The rule builder options.</returns>
    public static IRuleBuilderOptions<T, string?> ValidRequiredEnum<T, TEnum>(
        this IRuleBuilder<T, string?> ruleBuilder,
        IStringLocalizer<Messages> localizer,
        string propertyDisplayName = "Value")
        where TEnum : Enum
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage(_ => string.Format(localizer["FieldRequired"], propertyDisplayName))
            .Must(value => value!.ExistInEnum<TEnum>())
            .WithMessage(_ => string.Format(localizer["InvalidValue"], propertyDisplayName.ToLowerInvariant()));
    }

    /// <summary>
    /// Validates that an optional string value is null/empty or a valid enum value.
    /// Use for optional filter properties (e.g. optional category filter).
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <typeparam name="TEnum">The enum type to validate against.</typeparam>
    /// <param name="ruleBuilder">The rule builder.</param>
    /// <param name="propertyDisplayName">Display name for the property in error messages.</param>
    /// <returns>The rule builder options.</returns>
    public static IRuleBuilderOptions<T, string?> ValidOptionalEnum<T, TEnum>(
        this IRuleBuilder<T, string?> ruleBuilder,
        IStringLocalizer<Messages> localizer,
        string propertyDisplayName = "Value")
        where TEnum : Enum
    {
        return ruleBuilder
            .Must(value => string.IsNullOrWhiteSpace(value) || value.ExistInEnum<TEnum>())
            .WithMessage(_ => string.Format(localizer["InvalidValue"], propertyDisplayName.ToLowerInvariant()));
    }
}