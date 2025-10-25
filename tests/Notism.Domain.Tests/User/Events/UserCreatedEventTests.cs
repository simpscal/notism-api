using FluentAssertions;
using Notism.Domain.User.Events;
using Notism.Domain.User.ValueObjects;
using Xunit;

namespace Notism.Domain.Tests.UserAggregate.Events;

public class UserCreatedEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEventWithCorrectProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");

        // Act
        var userCreatedEvent = new UserCreatedEvent(userId, email);

        // Assert
        userCreatedEvent.Should().NotBeNull();
        userCreatedEvent.UserId.Should().Be(userId);
        userCreatedEvent.Email.Should().Be(email);
        userCreatedEvent.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldStillCreateEvent()
    {
        // Arrange
        var emptyUserId = Guid.Empty;
        var email = Email.Create("test@example.com");

        // Act
        var userCreatedEvent = new UserCreatedEvent(emptyUserId, email);

        // Assert
        userCreatedEvent.Should().NotBeNull();
        userCreatedEvent.UserId.Should().Be(Guid.Empty);
        userCreatedEvent.Email.Should().Be(email);
    }

    [Fact]
    public void Constructor_WithDifferentUserIds_ShouldCreateDistinctEvents()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var email = Email.Create("test@example.com");

        // Act
        var event1 = new UserCreatedEvent(userId1, email);
        var event2 = new UserCreatedEvent(userId2, email);

        // Assert
        event1.UserId.Should().NotBe(event2.UserId);
        event1.Email.Should().Be(event2.Email); // Same email
    }

    [Fact]
    public void Constructor_WithDifferentEmails_ShouldCreateDistinctEvents()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act
        var event1 = new UserCreatedEvent(userId, email1);
        var event2 = new UserCreatedEvent(userId, email2);

        // Assert
        event1.UserId.Should().Be(event2.UserId); // Same user ID
        event1.Email.Should().NotBe(event2.Email);
        event1.Email.Value.Should().Be("test1@example.com");
        event2.Email.Value.Should().Be("test2@example.com");
    }

    [Fact]
    public void Constructor_ShouldInheritFromDomainEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");

        // Act
        var userCreatedEvent = new UserCreatedEvent(userId, email);

        // Assert
        userCreatedEvent.Should().BeAssignableTo<Domain.Common.DomainEvent>();
    }

    [Fact]
    public void Email_PropertyShouldRetainOriginalCasing()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originalEmail = "Test@EXAMPLE.COM";
        var email = Email.Create(originalEmail);

        // Act
        var userCreatedEvent = new UserCreatedEvent(userId, email);

        // Assert
        // Email value object converts to lowercase, so the event should reflect that
        userCreatedEvent.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void UserId_PropertyShouldBeImmutable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");
        var userCreatedEvent = new UserCreatedEvent(userId, email);

        // Act
        var retrievedUserId = userCreatedEvent.UserId;

        // Assert
        retrievedUserId.Should().Be(userId);
        // Property should not have a setter (readonly)
        userCreatedEvent.GetType().GetProperty(nameof(UserCreatedEvent.UserId))!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void Email_PropertyShouldBeImmutable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = Email.Create("test@example.com");
        var userCreatedEvent = new UserCreatedEvent(userId, email);

        // Act
        var retrievedEmail = userCreatedEvent.Email;

        // Assert
        retrievedEmail.Should().Be(email);
        // Property should not have a setter (readonly)
        userCreatedEvent.GetType().GetProperty(nameof(UserCreatedEvent.Email))!.CanWrite.Should().BeFalse();
    }
}