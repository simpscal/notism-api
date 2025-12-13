using System.Security.Cryptography;

using AutoMapper;

using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Models;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.GoogleOAuth;

public class GoogleOAuthCallbackHandler : IRequestHandler<GoogleOAuthCallbackRequest, (AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly IGoogleOAuthService _googleOAuthService;

    public GoogleOAuthCallbackHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper,
        IGoogleOAuthService googleOAuthService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
        _googleOAuthService = googleOAuthService;
    }

    public async Task<(AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> Handle(
        GoogleOAuthCallbackRequest request,
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _googleOAuthService.ExchangeCodeForTokenAsync(
            request.Code,
            cancellationToken);

        var userInfo = await _googleOAuthService.GetUserInfoAsync(
            tokenResponse.AccessToken!,
            cancellationToken);

        var user = await _userRepository.FindByExpressionAsync(new UserByEmailSpecification(userInfo.Email!));

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
                throw new ResultFailureException("There was an error creating the user");
            }
        }

        var token = await _tokenService.GenerateTokenAsync(user);

        var response = _mapper.Map<AuthenticationResponse>(user);
        response.Token = token.Token;
        response.ExpiresAt = token.ExpiresAt;

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