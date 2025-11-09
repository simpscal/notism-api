using System.Security.Claims;

using MediatR;

using Notism.Api.Models;
using Notism.Api.Services;
using Notism.Application.Auth.Login;
using Notism.Application.Auth.RefreshToken;
using Notism.Application.Auth.Register;
using Notism.Application.Auth.RequestPasswordReset;
using Notism.Application.Auth.ResetPassword;
using Notism.Application.User.GetProfile;
using Notism.Shared.Exceptions;

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

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .AllowAnonymous()
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/reload", ReloadAsync)
            .WithName("Reload")
            .WithSummary("Get current user profile from access token")
            .WithDescription("Returns the user profile associated with the provided access token")
            .RequireAuthorization()
            .Produces<GetUserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPost("/request-password-reset", RequestPasswordResetAsync)
            .WithName("RequestPasswordReset")
            .WithSummary("Request a password reset token")
            .WithDescription("Sends a password reset email with a secure token to the user's email address")
            .AllowAnonymous()
            .Produces<RequestPasswordResetResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName("ResetPassword")
            .WithSummary("Reset password using reset token")
            .WithDescription("Resets the user's password using a valid reset token")
            .AllowAnonymous()
            .Produces<ResetPasswordResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IMediator mediator,
        ICookieService cookieService,
        HttpContext httpContext)
    {
        var result = await mediator.Send(request);

        cookieService.SetRefreshTokenCookie(
            httpContext,
            result.RefreshToken,
            result.RefreshTokenExpiresAt);

        await cookieService.GenerateAntiForgeryTokenAsync(httpContext);

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IMediator mediator,
        ICookieService cookieService,
        HttpContext httpContext)
    {
        var result = await mediator.Send(request);

        cookieService.SetRefreshTokenCookie(
            httpContext,
            result.RefreshToken,
            result.RefreshTokenExpiresAt);

        await cookieService.GenerateAntiForgeryTokenAsync(httpContext);

        return Results.Ok(result.Response);
    }

    private static async Task<IResult> RefreshTokenAsync(
        IMediator mediator,
        ICookieService cookieService,
        HttpContext httpContext)
    {
        await cookieService.ValidateAntiForgeryTokenAsync(httpContext);

        var refreshToken = cookieService.GetRefreshTokenFromCookie(httpContext);
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ResultFailureException("Refresh token not found");
        }

        var request = new RefreshTokenRequest { RefreshToken = refreshToken };
        var tokenResult = await mediator.Send(request);

        cookieService.SetRefreshTokenCookie(
            httpContext,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAt);

        return Results.Ok(new { Token = tokenResult.Token, ExpiresAt = tokenResult.ExpiresAt });
    }

    private static IResult LogoutAsync(
        ICookieService cookieService,
        HttpContext httpContext)
    {
        cookieService.ClearRefreshTokenCookie(httpContext);

        return Results.Ok(new { Message = "Logged out successfully" });
    }

    private static async Task<IResult> ReloadAsync(
        HttpContext httpContext,
        IMediator mediator)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new ResultFailureException("Invalid or missing user identifier in token");
        }

        var request = new GetUserProfileRequest { UserId = userId };
        var result = await mediator.Send(request);

        return Results.Ok(result);
    }

    private static async Task<IResult> RequestPasswordResetAsync(
        RequestPasswordResetRequest request,
        IMediator mediator)
    {
        var result = await mediator.Send(request);
        return Results.Ok(result);
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        IMediator mediator)
    {
        var result = await mediator.Send(request);
        return Results.Ok(result);
    }
}