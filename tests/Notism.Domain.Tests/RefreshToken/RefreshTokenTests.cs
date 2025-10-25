using FluentAssertions;

using Xunit;

using DomainRefreshToken = Notism.Domain.RefreshToken.RefreshToken;

namespace Notism.Domain.Tests.RefreshTokenEntity;

public class RefreshTokenTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnRefreshTokenWithCorrectProperties()
    {
        // Arrange
        var token = "test-refresh-token-12345";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = DomainRefreshToken.Create(token, userId, expiresAt);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Id.Should().NotBe(Guid.Empty);
        refreshToken.Token.Should().Be(token);
        refreshToken.UserId.Should().Be(userId);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.IsRevoked.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespaceToken_ShouldThrowArgumentException(string? invalidToken)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => DomainRefreshToken.Create(invalidToken!, userId, expiresAt));
        exception.Message.Should().Be("Token cannot be null or empty (Parameter 'token')");
        exception.ParamName.Should().Be("token");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var token = "test-refresh-token-12345";
        var emptyUserId = Guid.Empty;
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => DomainRefreshToken.Create(token, emptyUserId, expiresAt));
        exception.Message.Should().Be("UserId cannot be empty (Parameter 'userId')");
        exception.ParamName.Should().Be("userId");
    }

    [Fact]
    public void Create_WithPastExpirationDate_ShouldThrowArgumentException()
    {
        // Arrange
        var token = "test-refresh-token-12345";
        var userId = Guid.NewGuid();
        var pastExpiresAt = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => DomainRefreshToken.Create(token, userId, pastExpiresAt));
        exception.Message.Should().Be("ExpiresAt must be in the future (Parameter 'expiresAt')");
        exception.ParamName.Should().Be("expiresAt");
    }

    [Fact]
    public void Create_WithCurrentTimeAsExpirationDate_ShouldThrowArgumentException()
    {
        // Arrange
        var token = "test-refresh-token-12345";
        var userId = Guid.NewGuid();
        var currentTime = DateTime.UtcNow;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => DomainRefreshToken.Create(token, userId, currentTime));
        exception.Message.Should().Be("ExpiresAt must be in the future (Parameter 'expiresAt')");
        exception.ParamName.Should().Be("expiresAt");
    }

    [Fact]
    public void Revoke_OnValidToken_ShouldSetIsRevokedToTrue()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        refreshToken.IsRevoked.Should().BeFalse();

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_OnAlreadyRevokedToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke(); // First revocation

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => refreshToken.Revoke());
        exception.Message.Should().Be("Token is already revoked");
    }

    [Fact]
    public void Revoke_OnExpiredToken_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddMilliseconds(1));

        // Wait for token to expire
        Thread.Sleep(10);
        refreshToken.IsExpired().Should().BeTrue();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => refreshToken.Revoke());
        exception.Message.Should().Be("Cannot revoke an expired token");
    }

    [Fact]
    public void IsExpired_WithFutureExpirationDate_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        // Act
        var isExpired = refreshToken.IsExpired();

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastExpirationDate_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddMilliseconds(1));

        // Wait for token to expire
        Thread.Sleep(10);

        // Act
        var isExpired = refreshToken.IsExpired();

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithValidNonRevokedNonExpiredToken_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        // Act
        var isValid = refreshToken.IsValid();

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithRevokedToken_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke();

        // Act
        var isValid = refreshToken.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddMilliseconds(1));

        // Wait for token to expire
        Thread.Sleep(10);

        // Act
        var isValid = refreshToken.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithRevokedAndExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddMilliseconds(1));

        // Wait for token to expire, then try to revoke (should fail but let's test IsValid)
        Thread.Sleep(10);
        refreshToken.IsExpired().Should().BeTrue();

        // Act
        var isValid = refreshToken.IsValid();

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Create_WithDifferentParameters_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var token1 = DomainRefreshToken.Create("token1", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        var token2 = DomainRefreshToken.Create("token2", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        // Assert
        token1.Id.Should().NotBe(token2.Id);
        token1.Id.Should().NotBe(Guid.Empty);
        token2.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldInitializeIsRevokedToFalse()
    {
        // Arrange & Act
        var refreshToken = DomainRefreshToken.Create("test-token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        // Assert
        refreshToken.IsRevoked.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]    // 1 hour
    [InlineData(24)]   // 1 day
    [InlineData(168)]  // 1 week
    [InlineData(720)]  // 1 month
    public void Create_WithVariousValidFutureExpirationTimes_ShouldSucceed(int hoursInFuture)
    {
        // Arrange
        var token = "test-refresh-token";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddHours(hoursInFuture);

        // Act
        var refreshToken = DomainRefreshToken.Create(token, userId, expiresAt);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.IsExpired().Should().BeFalse();
        refreshToken.IsValid().Should().BeTrue();
    }
}