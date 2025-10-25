using FluentAssertions;
using NSubstitute;
using Notism.Application.Auth.RefreshToken;
using Notism.Application.Common.Interfaces;
using Notism.Shared.Exceptions;
using Xunit;

namespace Notism.Application.Tests.Auth.RefreshToken;

public class SimpleRefreshTokenUseCaseTests : BaseTest
{
    private readonly RefreshTokenUseCase _refreshTokenUseCase;

    public SimpleRefreshTokenUseCaseTests()
    {
        _refreshTokenUseCase = new RefreshTokenUseCase(MockTokenService);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid-refresh-token-12345"
        };

        var expectedTokenResult = CreateTestTokenResult();

        MockTokenService
            .RefreshTokenAsync(request.RefreshToken)
            .Returns(expectedTokenResult);

        // Act
        var result = await _refreshTokenUseCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Token.Should().Be(expectedTokenResult.Token);
        result.Value.RefreshToken.Should().Be(expectedTokenResult.RefreshToken);

        await MockTokenService.Received(1).RefreshTokenAsync(request.RefreshToken);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldThrowResultFailureException()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        MockTokenService
            .RefreshTokenAsync(request.RefreshToken)
            .Returns<TokenResult>(_ => throw new InvalidRefreshTokenException("Invalid refresh token"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ResultFailureException>(
            () => _refreshTokenUseCase.Handle(request, CancellationToken.None));

        exception.Message.Should().Be("Invalid refresh token");
        await MockTokenService.Received(1).RefreshTokenAsync(request.RefreshToken);
    }
}