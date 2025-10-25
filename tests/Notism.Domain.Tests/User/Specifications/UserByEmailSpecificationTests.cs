using System.Linq.Expressions;

using FluentAssertions;

using Notism.Domain.User.Specifications;

using Xunit;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Domain.Tests.UserAggregate.Specifications;

public class UserByEmailSpecificationTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateSpecification()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var specification = new UserByEmailSpecification(email);

        // Assert
        specification.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string? invalidEmail)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new UserByEmailSpecification(invalidEmail!));
        exception.Message.Should().Be("Email cannot be empty (Parameter 'email')");
    }

    [Fact]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEmailFormat = "invalid-email-format";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new UserByEmailSpecification(invalidEmailFormat));
        exception.Message.Should().Be("Invalid email format (Parameter 'email')");
    }

    [Fact]
    public void ToExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        var email = "test@example.com";
        var specification = new UserByEmailSpecification(email);

        // Act
        var expression = specification.ToExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Should().BeAssignableTo<Expression<Func<DomainUser, bool>>>();
    }

    [Fact]
    public void ToExpression_WhenCompiledAndExecuted_ShouldReturnTrueForMatchingUser()
    {
        // Arrange
        var email = "test@example.com";
        var specification = new UserByEmailSpecification(email);
        var user = DomainUser.Create(email, "TestPassword123!");

        // Act
        var expression = specification.ToExpression();
        var compiledExpression = expression.Compile();
        var result = compiledExpression(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ToExpression_WhenCompiledAndExecuted_ShouldReturnFalseForNonMatchingUser()
    {
        // Arrange
        var specificationEmail = "test@example.com";
        var userEmail = "different@example.com";
        var specification = new UserByEmailSpecification(specificationEmail);
        var user = DomainUser.Create(userEmail, "TestPassword123!");

        // Act
        var expression = specification.ToExpression();
        var compiledExpression = expression.Compile();
        var result = compiledExpression(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToExpression_ShouldBeCaseInsensitive()
    {
        // Arrange
        var specificationEmail = "Test@EXAMPLE.COM";
        var userEmail = "test@example.com";
        var specification = new UserByEmailSpecification(specificationEmail);
        var user = DomainUser.Create(userEmail, "TestPassword123!");

        // Act
        var expression = specification.ToExpression();
        var compiledExpression = expression.Compile();
        var result = compiledExpression(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithMatchingUser_ShouldReturnTrue()
    {
        // Arrange
        var email = "test@example.com";
        var specification = new UserByEmailSpecification(email);
        var user = DomainUser.Create(email, "TestPassword123!");

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithNonMatchingUser_ShouldReturnFalse()
    {
        // Arrange
        var specificationEmail = "test@example.com";
        var userEmail = "different@example.com";
        var specification = new UserByEmailSpecification(specificationEmail);
        var user = DomainUser.Create(userEmail, "TestPassword123!");

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldBeCaseInsensitive()
    {
        // Arrange
        var specificationEmail = "Test@EXAMPLE.COM";
        var userEmail = "test@example.com";
        var specification = new UserByEmailSpecification(specificationEmail);
        var user = DomainUser.Create(userEmail, "TestPassword123!");

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithValidEmailFormats_ShouldCreateSpecification()
    {
        // Arrange & Act & Assert
        var validEmails = new[]
        {
            "test@example.com",
            "user123@domain.org",
            "first.last@company.co.uk",
            "test+tag@example.com",
            "test_user@example-domain.com"
        };

        foreach (var email in validEmails)
        {
            var specification = new UserByEmailSpecification(email);
            specification.Should().NotBeNull();
        }
    }
}