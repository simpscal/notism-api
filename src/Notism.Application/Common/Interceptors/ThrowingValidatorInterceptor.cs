using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;

namespace Notism.Application.Common.Interceptors;

public class ThrowingValidatorInterceptor : IValidatorInterceptor
{
    public IValidationContext BeforeAspNetValidation(
        ActionContext actionContext,
        IValidationContext commonContext)
    {
        return commonContext;
    }

    public ValidationResult AfterAspNetValidation(
        ActionContext actionContext,
        IValidationContext validationContext,
        ValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        return result;
    }
}