using System.Net;
using System.Text.Json;

using Notism.Api.Models;
using Notism.Shared.Exceptions;

using OneOf.Types;

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

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ResultFailureException ex)
        {
            await HandleResultFailureAsync(context, ex);
        }
    }

    private async Task HandleResultFailureAsync(HttpContext context, ResultFailureException exception)
    {
        _logger.LogWarning("Result failure: {ErrorMessage}", exception.ErrorMessage);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new ErrorResponse
        {
            Message = exception.ErrorMessage,
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}