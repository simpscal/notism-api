using AutoMapper;

using MediatR;

using Notism.Application.Auth.Models;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.Login;

public class LoginHandler : IRequestHandler<LoginRequest, (AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly IMessages _messages;

    public LoginHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper,
        IMessages messages)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
        _messages = messages;
    }

    public async Task<(AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var email = Email.Create(request.Email);
        var specification = new FilterSpecification<Domain.User.User>(u => u.Email.Equals(email));
        var user = await _userRepository.FindByExpressionAsync(specification)
            ?? throw new ResultFailureException(_messages.InvalidCredentials);

        // 2. Verify password
        if (!_passwordService.VerifyPassword(user.Password, request.Password))
        {
            throw new ResultFailureException(_messages.InvalidCredentials);
        }

        // 3. Generate JWT token
        var token = await _tokenService.GenerateTokenAsync(user);

        // 4. Map to response using AutoMapper
        var response = _mapper.Map<AuthenticationResponse>(user);
        response.Token = token.Token;
        response.ExpiresAt = token.ExpiresAt;

        return (response, token.RefreshToken, token.RefreshTokenExpiresAt);
    }
}