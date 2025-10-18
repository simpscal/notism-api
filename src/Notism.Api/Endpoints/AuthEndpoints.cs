using MediatR;

using Notism.Api.Models;
using Notism.Application.Auth.Login;
using Notism.Application.Auth.Register;

namespace Notism.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user and return JWT token")
            .WithDescription("Authenticates a user with email and password, returns JWT token and user information")
            .AllowAnonymous()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user and return JWT token")
            .WithDescription("Creates a new user account and returns JWT token and user information")
            .AllowAnonymous()
            .Produces<RegisterResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IMediator mediator)
    {
        var result = await mediator.Send(request);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IMediator mediator)
    {
        var result = await mediator.Send(request);
        return Results.Ok(result.Value);
    }
}