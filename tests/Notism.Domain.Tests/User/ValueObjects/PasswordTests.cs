using FluentAssertions;
using Notism.Domain.User.ValueObjects;
using Xunit;

namespace Notism.Domain.Tests.UserAggregate.ValueObjects;

public class PasswordTests
{
    [Fact]
    public void Create_WithValidPassword_ShouldReturnPasswordValueObject()
    {
        // Arrange
        var passwordString = "TestPassword123!";

        // Act
        var password = Password.Create(passwordString);

        // Assert
        password.Should().NotBeNull();
        password.Value.Should().Be("TestPassword123!");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespacePassword_ShouldThrowArgumentException(string? passwordString)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Password.Create(passwordString!));
        exception.Message.Should().Be("Password cannot be empty (Parameter 'password')");
        exception.ParamName.Should().Be("password");
    }

    [Theory]
    [InlineData("1234567")] // 7 characters
    [InlineData("abc")]     // 3 characters
    [InlineData("A")]       // 1 character
    [InlineData("")]        // Empty string (also caught by null/whitespace check)
    public void Create_WithPasswordShorterThan8Characters_ShouldThrowArgumentException(string passwordString)
    {
        // Act & Assert
        if (string.IsNullOrWhiteSpace(passwordString))
        {
            var exception = Assert.Throws<ArgumentException>(() => Password.Create(passwordString));
            exception.Message.Should().Be("Password cannot be empty (Parameter 'password')");
        }
        else
        {
            var exception = Assert.Throws<ArgumentException>(() => Password.Create(passwordString));
            exception.Message.Should().Be("Password must be at least 8 characters long (Parameter 'password')");
            exception.ParamName.Should().Be("password");
        }
    }

    [Theory]
    [InlineData("12345678")]                    // Exactly 8 characters
    [InlineData("TestPassword123!")]            // Strong password
    [InlineData("simple123")]                   // Simple but valid length
    [InlineData("VeryLongPasswordWithManyCharacters123!@#$%^&*()")]  // Very long password
    [InlineData("12345678901234567890")]        // 20 characters
    public void Create_WithValidPasswordLength_ShouldSucceed(string passwordString)
    {
        // Act
        var password = Password.Create(passwordString);

        // Assert
        password.Should().NotBeNull();
        password.Value.Should().Be(passwordString);
    }

    [Fact]
    public void ImplicitStringConversion_ShouldReturnPasswordValue()
    {
        // Arrange
        var password = Password.Create("TestPassword123!");

        // Act
        string passwordString = password;

        // Assert
        passwordString.Should().Be("TestPassword123!");
    }

    [Fact]
    public void Equals_WithSamePasswordValue_ShouldReturnTrue()
    {
        // Arrange
        var password1 = Password.Create("TestPassword123!");
        var password2 = Password.Create("TestPassword123!");

        // Act & Assert
        password1.Should().Be(password2);
        password1.Equals(password2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentPasswordValue_ShouldReturnFalse()
    {
        // Arrange
        var password1 = Password.Create("TestPassword123!");
        var password2 = Password.Create("DifferentPassword456!");

        // Act & Assert
        password1.Should().NotBe(password2);
        password1.Equals(password2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSamePasswordValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var password1 = Password.Create("TestPassword123!");
        var password2 = Password.Create("TestPassword123!");

        // Act & Assert
        password1.GetHashCode().Should().Be(password2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentPasswordValue_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var password1 = Password.Create("TestPassword123!");
        var password2 = Password.Create("DifferentPassword456!");

        // Act & Assert
        password1.GetHashCode().Should().NotBe(password2.GetHashCode());
    }

    [Fact]
    public void Create_WithPasswordContainingSpecialCharacters_ShouldSucceed()
    {
        // Arrange
        var passwordString = "P@ssw0rd!#$%^&*()_+-=[]{}|;:,.<>?";

        // Act
        var password = Password.Create(passwordString);

        // Assert
        password.Should().NotBeNull();
        password.Value.Should().Be(passwordString);
    }

    [Fact]
    public void Create_WithPasswordContainingSpaces_ShouldSucceed()
    {
        // Arrange
        var passwordString = "Pass word 123!";

        // Act
        var password = Password.Create(passwordString);

        // Assert
        password.Should().NotBeNull();
        password.Value.Should().Be(passwordString);
    }

    [Fact]
    public void Create_WithPasswordContainingUnicodeCharacters_ShouldSucceed()
    {
        // Arrange
        var passwordString = "Pássw0rd123!ñü";

        // Act
        var password = Password.Create(passwordString);

        // Assert
        password.Should().NotBeNull();
        password.Value.Should().Be(passwordString);
    }
}