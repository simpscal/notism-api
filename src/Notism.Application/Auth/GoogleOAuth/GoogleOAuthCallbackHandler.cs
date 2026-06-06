using System.Security.Cryptography;

using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthCallbackHandler : IRequestHandler<GoogleOAuthCallbackRequest, (GoogleOAuthCallbackResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IMessages _messages;

    public GoogleOAuthCallbackHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IGoogleOAuthService googleOAuthService,
        IMessages messages)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _googleOAuthService = googleOAuthService;
        _messages = messages;
    }

    public async Task<(GoogleOAuthCallbackResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> Handle(
        GoogleOAuthCallbackRequest request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(
            request.Code,
            cancellationToken);

        var userInfo = await _googleOAuthService.GetUserInfoAsync(
            tokenResponse.AccessToken!,
            cancellationToken);

        var email = Email.Create(userInfo.Email!);
        var specification = new FilterSpecification<Domain.User.User>(u => u.Email.Equals(email));
        var user = await _userRepository.FindByExpressionAsync(specification);

        if (user == null)
        {
            // Create new user with a random password (OAuth users don't need password)
            var randomPassword = GenerateRandomPassword();
            var hashedPassword = _passwordService.HashPassword(randomPassword);
            user = Domain.User.User.Create(
                userInfo.Email!,
                hashedPassword,
                UserRole.User,
                userInfo.GivenName,
                userInfo.FamilyName,
                userInfo.Picture);

            await _userRepository.AddAsync(user);

            if (await _userRepository.SaveChangesAsync() < 1)
            {
                throw new ResultFailureException(_messages.ErrorCreatingUser);
            }
        }

        var token = await _tokenService.GenerateTokenAsync(user);

        var response = GoogleOAuthCallbackResponse.FromDomain(user, token);

        return (response, token.RefreshToken, token.RefreshTokenExpiresAt);
    }

    private static string GenerateRandomPassword()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}