using FluentAssertions;
using NSubstitute;
using Notism.Application.Auth.Register;
using Notism.Application.Common.Interfaces;
using Notism.Domain.User;
using Notism.Domain.User.Specifications;
using Notism.Shared.Exceptions;
using Xunit;

namespace Notism.Application.Tests.Auth.Register;

public class RegisterUseCaseTests : BaseTest
{
    private readonly RegisterUseCase _registerUseCase;

    public RegisterUseCaseTests()
    {
        _registerUseCase = new RegisterUseCase(
            MockUserRepository,
            MockTokenService,
            MockMapper);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "TestPassword123!",
        };

        var tokenResult = CreateTestTokenResult();

        MockUserRepository
            .FindByExpressionAsync(Arg.Any<UserByEmailSpecification>())
            .Returns((User?)null);

        MockUserRepository
            .AddAsync(Arg.Any<User>())
            .Returns(Task.FromResult(CreateTestUser()));

        MockUserRepository
            .SaveChangesAsync()
            .Returns(1);

        MockTokenService
            .GenerateTokenAsync(Arg.Any<User>())
            .Returns(tokenResult);

        MockMapper
            .Map<RegisterResponse>(Arg.Any<User>())
            .Returns(new RegisterResponse
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                Token = string.Empty,
                ExpiresAt = default,
                RefreshToken = string.Empty,
                RefreshTokenExpiresAt = default,
            });

        // Act
        var result = await _registerUseCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(request.Email);
        result.Value.Token.Should().Be(tokenResult.Token);
        result.Value.RefreshToken.Should().Be(tokenResult.RefreshToken);

        await MockUserRepository.Received(1).FindByExpressionAsync(Arg.Any<UserByEmailSpecification>());
        await MockUserRepository.Received(1).AddAsync(Arg.Any<User>());
        await MockUserRepository.Received(1).SaveChangesAsync();
        await MockTokenService.Received(1).GenerateTokenAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowResultFailureException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "TestPassword123!",
        };

        var existingUser = CreateTestUser();

        MockUserRepository
            .FindByExpressionAsync(Arg.Any<UserByEmailSpecification>())
            .Returns(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ResultFailureException>(
            () => _registerUseCase.Handle(request, CancellationToken.None));

        exception.Message.Should().Be("User with this email already exists");
        await MockUserRepository.Received(1).FindByExpressionAsync(Arg.Any<UserByEmailSpecification>());
        await MockUserRepository.DidNotReceive().AddAsync(Arg.Any<User>());
        await MockUserRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldThrowResultFailureException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "TestPassword123!",
        };

        MockUserRepository
            .FindByExpressionAsync(Arg.Any<UserByEmailSpecification>())
            .Returns((User?)null);

        MockUserRepository
            .AddAsync(Arg.Any<User>())
            .Returns(Task.FromResult(CreateTestUser()));

        MockUserRepository
            .SaveChangesAsync()
            .Returns(0);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ResultFailureException>(
            () => _registerUseCase.Handle(request, CancellationToken.None));

        exception.Message.Should().Be("There was an error creating the user");
        await MockUserRepository.Received(1).FindByExpressionAsync(Arg.Any<UserByEmailSpecification>());
        await MockUserRepository.Received(1).AddAsync(Arg.Any<User>());
        await MockUserRepository.Received(1).SaveChangesAsync();
        await MockTokenService.DidNotReceive().GenerateTokenAsync(Arg.Any<User>());
    }
}