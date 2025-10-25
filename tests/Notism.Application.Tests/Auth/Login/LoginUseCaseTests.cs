using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Notism.Application.Auth.Login;
using Notism.Application.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Domain.User.ValueObjects;
using Notism.Shared.Exceptions;
using Xunit;

namespace Notism.Application.Tests.Auth.Login;

public class LoginUseCaseTests : BaseTest
{
    private readonly LoginUseCase _loginUseCase;

    public LoginUseCaseTests()
    {
        _loginUseCase = new LoginUseCase(
            MockUserRepository,
            MockTokenService,
            MockPasswordService,
            MockMapper);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!",
        };

        var user = CreateTestUser();
        var tokenResult = CreateTestTokenResult();

        MockUserRepository
            .FindByExpressionAsync(Arg.Any<UserByEmailSpecification>())
            .Returns(user);

        MockPasswordService
            .VerifyPassword(user.Password, Arg.Any<string>())
            .Returns(true);

        MockTokenService
            .GenerateTokenAsync(Arg.Any<User>())
            .Returns(tokenResult);

        MockMapper
            .Map<LoginResponse>(Arg.Any<User>())
            .Returns(new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email.Value,
                Token = string.Empty,
                ExpiresAt = default,
                RefreshToken = string.Empty,
                RefreshTokenExpiresAt = default,
            });

        // Act
        var result = await _loginUseCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(request.Email);
        result.Value.Token.Should().Be(tokenResult.Token);
        result.Value.RefreshToken.Should().Be(tokenResult.RefreshToken);

        await MockUserRepository.Received(1).FindByExpressionAsync(Arg.Any<UserByEmailSpecification>());
        MockPasswordService.Received(1).VerifyPassword(user.Password, Arg.Any<string>());
        await MockTokenService.Received(1).GenerateTokenAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowResultFailureException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "TestPassword123!",
        };

        MockUserRepository
            .FindByExpressionAsync(Arg.Any<UserByEmailSpecification>())
            .Returns((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ResultFailureException>(
            () => _loginUseCase.Handle(request, CancellationToken.None));

        exception.Message.Should().Be("Invalid email or password");
        await MockUserRepository.Received(1).FindByExpressionAsync(Arg.Any<UserByEmailSpecification>());
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowResultFailureException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword",
        };

        var user = CreateTestUser();

        MockUserRepository
            .FindByExpressionAsync(Arg.Any<UserByEmailSpecification>())
            .Returns(user);

        MockPasswordService
            .VerifyPassword(user.Password, Arg.Any<string>())
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ResultFailureException>(
            () => _loginUseCase.Handle(request, CancellationToken.None));

        exception.Message.Should().Be("Invalid email or password");
        await MockUserRepository.Received(1).FindByExpressionAsync(Arg.Any<UserByEmailSpecification>());
        MockPasswordService.Received(1).VerifyPassword(user.Password, Arg.Any<string>());
    }
}