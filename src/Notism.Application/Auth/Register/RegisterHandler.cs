using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.User.Enums;
using Notism.Domain.User.Repositories;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.Auth.Register;

public class RegisterHandler : IRequestHandler<RegisterRequest, (RegisterResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    private readonly IUserRepository _userRepository;
    private readonly IReadDbContext _readDbContext;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMessages _messages;

    public RegisterHandler(
        IUserRepository userRepository,
        IReadDbContext readDbContext,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMessages messages)
    {
        _userRepository = userRepository;
        _readDbContext = readDbContext;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _messages = messages;
    }

    public async Task<(RegisterResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var existingUser = await _readDbContext.Set<DomainUser>()
            .Where(u => u.Email.Equals(email))
            .AnyAsync(cancellationToken);

        if (existingUser)
        {
            throw new ResultFailureException(_messages.UserAlreadyExists);
        }

        var hashedPassword = _passwordService.HashPassword(request.Password);
        var user = Domain.User.User.Create(request.Email, hashedPassword, UserRole.User, request.FirstName, request.LastName);

        await _userRepository.AddAsync(user);

        if (await _userRepository.SaveChangesAsync() < 1)
        {
            throw new ResultFailureException(_messages.ErrorCreatingUser);
        }

        var token = await _tokenService.GenerateTokenAsync(user);
        var response = RegisterResponse.FromDomain(user, token);

        return (response, token.RefreshToken, token.RefreshTokenExpiresAt);
    }
}