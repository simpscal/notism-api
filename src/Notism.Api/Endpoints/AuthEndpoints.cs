using MediatR;

using Notism.Api.Extensions;
using Notism.Api.Interfaces;
using Notism.Api.Models;
using Notism.Application.Auth.GoogleOAuth;
using Notism.Application.Auth.Login;
using Notism.Application.Auth.RefreshToken;
using Notism.Application.Auth.Register;
using Notism.Application.Auth.RequestPasswordReset;
using Notism.Application.Auth.ResetPassword;
using Notism.Application.Common.Models;
using Notism.Application.User.GetProfile;
using Notism.Domain.RefreshToken;
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
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
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
            .RequireAuthorization()
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

        group.MapGet("/google/redirect", GoogleOAuthRedirectAsync)
            .WithName("GoogleOAuthRedirect")
            .WithSummary("Get Google OAuth redirect URL")
            .WithDescription("Returns a redirect URL for Google OAuth authentication. The redirect URL after authentication will be {appDomain}/auth/oauth/callback")
            .AllowAnonymous()
            .Produces<GoogleOAuthRedirectResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/google/callback", GoogleOAuthCallbackAsync)
            .WithName("GoogleOAuthCallback")
            .WithSummary("Handle Google OAuth callback")
            .WithDescription("Receives code and state from Google OAuth, verifies the user, and returns the same response as login API")
            .AllowAnonymous()
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
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

        return Results.Ok(new { Token = tokenResult.Token, ExpiresAt = tokenResult.ExpiresAt });
    }

    private static async Task<IResult> LogoutAsync(
        ICookieService cookieService,
        IRefreshTokenRepository refreshTokenRepository,
        HttpContext httpContext)
    {
        var userId = httpContext.User.GetUserId();

        await refreshTokenRepository.RevokeAllUserTokensAsync(userId);

        cookieService.ClearAuthenticationCookies(httpContext);

        return Results.Ok(new { Message = "Logged out successfully" });
    }

    private static async Task<IResult> ReloadAsync(
        HttpContext httpContext,
        IMediator mediator)
    {
        var userId = httpContext.User.GetUserId();

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

    private static async Task<IResult> GoogleOAuthRedirectAsync(
        IMediator mediator)
    {
        var request = new GoogleOAuthRedirectRequest();
        var result = await mediator.Send(request);
        return Results.Ok(result);
    }

    private static async Task<IResult> GoogleOAuthCallbackAsync(
        GoogleOAuthCallbackRequest request,
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
}