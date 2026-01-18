using AutoMapper;

using MediatR;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Models;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.Login;

public class LoginHandler : IRequestHandler<LoginRequest, (AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public LoginHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<(AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var user = await _userRepository.FindByExpressionAsync(new UserByEmailSpecification(request.Email))
        ?? throw new ResultFailureException("Invalid email or password");

        // 2. Verify password
        if (!_passwordService.VerifyPassword(user.Password, request.Password))
        {
            throw new ResultFailureException("Invalid email or password");
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