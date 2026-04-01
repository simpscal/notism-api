using AutoMapper;

using MediatR;

using Notism.Application.Auth.Models;
using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;

namespace Notism.Application.Auth.Register;

public class RegisterHandler : IRequestHandler<RegisterRequest, (AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly IMessages _messages;

    public RegisterHandler(
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

    public async Task<(AuthenticationResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        // 1. Check if user already exists
        var email = Email.Create(request.Email);
        var specification = new FilterSpecification<Domain.User.User>(u => u.Email.Equals(email));
        var existingUser = await _userRepository.FindByExpressionAsync(specification);

        if (existingUser != null)
        {
            throw new ResultFailureException(_messages.UserAlreadyExists);
        }

        // 2. Create new user with hashed password
        var hashedPassword = _passwordService.HashPassword(request.Password);
        var user = Domain.User.User.Create(request.Email, hashedPassword, UserRole.User, request.FirstName, request.LastName);

        await _userRepository.AddAsync(user);

        if (await _userRepository.SaveChangesAsync() < 1)
        {
            throw new ResultFailureException(_messages.ErrorCreatingUser);
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