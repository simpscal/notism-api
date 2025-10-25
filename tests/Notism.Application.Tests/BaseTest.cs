using AutoMapper;
using NSubstitute;
using Notism.Application.Common.Interfaces;
using Notism.Domain.RefreshToken;
using Notism.Domain.User;

namespace Notism.Application.Tests;

public abstract class BaseTest
{
    protected IUserRepository MockUserRepository { get; }
    protected IRefreshTokenRepository MockRefreshTokenRepository { get; }
    protected ITokenService MockTokenService { get; }
    protected IPasswordService MockPasswordService { get; }
    protected IMapper MockMapper { get; }

    protected BaseTest()
    {
        MockUserRepository = Substitute.For<IUserRepository>();
        MockRefreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        MockTokenService = Substitute.For<ITokenService>();
        MockPasswordService = Substitute.For<IPasswordService>();
        MockMapper = Substitute.For<IMapper>();
    }

    protected User CreateTestUser()
    {
        return User.Create("test@example.com", "TestPassword123!");
    }

    protected TokenResult CreateTestTokenResult()
    {
        return new TokenResult
        {
            Token = "test-jwt-token-12345",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshToken = "test-refresh-token-12345",
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
        };
    }

    protected RefreshToken CreateTestRefreshToken(Guid? userId = null)
    {
        return RefreshToken.Create(
            "test-refresh-token-12345",
            userId ?? Guid.NewGuid(),
            DateTime.UtcNow.AddDays(7));
    }
}