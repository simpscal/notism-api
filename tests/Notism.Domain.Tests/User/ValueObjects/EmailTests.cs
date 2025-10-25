using FluentAssertions;
using Notism.Domain.User.ValueObjects;
using Xunit;

namespace Notism.Domain.Tests.UserAggregate.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnEmailValueObject()
    {
        // Arrange
        var emailString = "test@example.com";

        // Act
        var email = Email.Create(emailString);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithValidEmailMixedCase_ShouldReturnLowercaseEmail()
    {
        // Arrange
        var emailString = "Test@EXAMPLE.COM";

        // Act
        var email = Email.Create(emailString);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespaceEmail_ShouldThrowArgumentException(string? emailString)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Email.Create(emailString!));
        exception.Message.Should().Be("Email cannot be empty (Parameter 'email')");
        exception.ParamName.Should().Be("email");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    [InlineData("test@@example.com")]
    [InlineData("test @example.com")]
    [InlineData("test@ex ample.com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowArgumentException(string emailString)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Email.Create(emailString));
        exception.Message.Should().Be("Invalid email format (Parameter 'email')");
        exception.ParamName.Should().Be("email");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user123@domain.org")]
    [InlineData("first.last@company.co.uk")]
    [InlineData("test+tag@example.com")]
    [InlineData("test_user@example-domain.com")]
    public void Create_WithValidEmailFormats_ShouldSucceed(string emailString)
    {
        // Act
        var email = Email.Create(emailString);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(emailString.ToLowerInvariant());
    }

    [Fact]
    public void ImplicitStringConversion_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void Equals_WithSameEmailValue_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.Equals(email2).Should().BeTrue();
        // Value objects should have value equality, not reference equality
        ReferenceEquals(email1, email2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentEmailValue_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        email1.Equals(email2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameEmailValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentEmailValue_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.GetHashCode().Should().NotBe(email2.GetHashCode());
    }
}