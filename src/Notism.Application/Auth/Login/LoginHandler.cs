using MediatR;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Application.Common.Services;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Application.Auth.Login;

public class LoginHandler : IRequestHandler<LoginRequest, (LoginResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMessages _messages;

    public LoginHandler(
        IReadDbContext readDbContext,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMessages messages)
    {
        _readDbContext = readDbContext;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _messages = messages;
    }

    public async Task<(LoginResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt)> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await _readDbContext.Set<DomainUser>()
                .Where(u => u.Email.Equals(email))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResultFailureException(_messages.InvalidCredentials);

        if (!_passwordService.VerifyPassword(user.Password, request.Password))
        {
            throw new ResultFailureException(_messages.InvalidCredentials);
        }

        var token = await _tokenService.GenerateTokenAsync(user);
        var response = LoginResponse.FromDomain(user, token);

        return (response, token.RefreshToken, token.RefreshTokenExpiresAt);
    }
}