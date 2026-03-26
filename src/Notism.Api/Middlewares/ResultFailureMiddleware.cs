using System.Net;
using System.Text.Json;

using Notism.Api.Models;
using Notism.Application.Common.Services;
using Notism.Shared.Exceptions;

namespace Notism.Api.Middlewares;

public class ResultFailureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResultFailureMiddleware> _logger;

    public ResultFailureMiddleware(RequestDelegate next, ILogger<ResultFailureMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMessages messages)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, messages);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, IMessages messages)
    {
        var (statusCode, message, errors) = exception switch
        {
            ResultFailureException resultEx => (HttpStatusCode.BadRequest, resultEx.ErrorMessage, null),
            ValidationException validationEx => (HttpStatusCode.BadRequest, validationEx.Message, validationEx.Errors),
            NotFoundException notFoundEx => (HttpStatusCode.NotFound, notFoundEx.Message, null),
            UnauthorizedException unauthorizedEx => (HttpStatusCode.Unauthorized, unauthorizedEx.Message, null),
            ForbiddenException forbiddenEx => (HttpStatusCode.Forbidden, forbiddenEx.Message, null),
            InvalidRefreshTokenException refreshEx => (HttpStatusCode.Unauthorized, refreshEx.Message, null),
            _ => (HttpStatusCode.InternalServerError, messages.UnexpectedError, null),
        };

        _logger.LogWarning(
            exception,
            "Exception handled: {ExceptionType} - Status: {StatusCode} - Message: {Message}",
            exception.GetType().Name,
            (int)statusCode,
            message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Message = message,
            Errors = errors != null && errors.Any() ? errors : null,
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}