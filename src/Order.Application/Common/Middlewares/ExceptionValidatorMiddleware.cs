using System.Net;

using FluentValidation;

using Microsoft.AspNetCore.Http;

using Order.Application.Common.Models;

namespace Order.Application.Common.Middlewares;

public class ExceptionValidatorMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var response = new AppError(
                HttpStatusCode.BadRequest,
                exception.Errors.Select(error => error.ErrorMessage));

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var response = new AppError(
                HttpStatusCode.BadRequest,
                [exception.Message]);

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}