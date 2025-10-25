using System.Linq.Expressions;

using FluentAssertions;

using Notism.Domain.User.Specifications;

using Xunit;

using DomainUser = Notism.Domain.User.User;

namespace Notism.Domain.Tests.UserAggregate.Specifications;

public class UserByIdSpecificationTests
{
    [Fact]
    public void Constructor_WithValidUserId_ShouldCreateSpecification()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var specification = new UserByIdSpecification(userId);

        // Assert
        specification.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldCreateSpecification()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var specification = new UserByIdSpecification(emptyGuid);

        // Assert
        specification.Should().NotBeNull();
    }

    [Fact]
    public void ToExpression_ShouldReturnCorrectExpression()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var specification = new UserByIdSpecification(userId);

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
        var user = DomainUser.Create("test@example.com", "TestPassword123!");
        var specification = new UserByIdSpecification(user.Id);

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
        var user = DomainUser.Create("test@example.com", "TestPassword123!");
        var differentUserId = Guid.NewGuid();
        var specification = new UserByIdSpecification(differentUserId);

        // Act
        var expression = specification.ToExpression();
        var compiledExpression = expression.Compile();
        var result = compiledExpression(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithMatchingUser_ShouldReturnTrue()
    {
        // Arrange
        var user = DomainUser.Create("test@example.com", "TestPassword123!");
        var specification = new UserByIdSpecification(user.Id);

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_WithNonMatchingUser_ShouldReturnFalse()
    {
        // Arrange
        var user = DomainUser.Create("test@example.com", "TestPassword123!");
        var differentUserId = Guid.NewGuid();
        var specification = new UserByIdSpecification(differentUserId);

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_WithEmptyGuidSpecification_ShouldReturnFalseForAnyUser()
    {
        // Arrange
        var user = DomainUser.Create("test@example.com", "TestPassword123!");
        var specification = new UserByIdSpecification(Guid.Empty);

        // Act
        var result = specification.IsSatisfiedBy(user);

        // Assert
        result.Should().BeFalse();
        user.Id.Should().NotBe(Guid.Empty); // User should have a real ID
    }

    [Fact]
    public void ToExpression_AndIsSatisfiedBy_ShouldGiveSameResults()
    {
        // Arrange
        var user1 = DomainUser.Create("test1@example.com", "TestPassword123!");
        var user2 = DomainUser.Create("test2@example.com", "TestPassword123!");
        var specification = new UserByIdSpecification(user1.Id);

        // Act
        var expression = specification.ToExpression();
        var compiledExpression = expression.Compile();

        var expressionResult1 = compiledExpression(user1);
        var expressionResult2 = compiledExpression(user2);

        var satisfiedByResult1 = specification.IsSatisfiedBy(user1);
        var satisfiedByResult2 = specification.IsSatisfiedBy(user2);

        // Assert
        expressionResult1.Should().Be(satisfiedByResult1);
        expressionResult2.Should().Be(satisfiedByResult2);

        expressionResult1.Should().BeTrue();
        expressionResult2.Should().BeFalse();
        satisfiedByResult1.Should().BeTrue();
        satisfiedByResult2.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMultipleDifferentUserIds_ShouldCreateDistinctSpecifications()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var user = DomainUser.Create("test@example.com", "TestPassword123!");

        // Act
        var specification1 = new UserByIdSpecification(userId1);
        var specification2 = new UserByIdSpecification(userId2);
        var specification3 = new UserByIdSpecification(user.Id);

        // Assert
        specification1.IsSatisfiedBy(user).Should().BeFalse();
        specification2.IsSatisfiedBy(user).Should().BeFalse();
        specification3.IsSatisfiedBy(user).Should().BeTrue();
    }

    [Fact]
    public void ToExpression_ShouldBeReusable()
    {
        // Arrange
        var user1 = DomainUser.Create("test1@example.com", "TestPassword123!");
        var user2 = DomainUser.Create("test2@example.com", "TestPassword123!");
        var specification = new UserByIdSpecification(user1.Id);

        // Act
        var expression = specification.ToExpression();
        var compiledExpression = expression.Compile();

        var result1 = compiledExpression(user1);
        var result2 = compiledExpression(user2);
        var result1Again = compiledExpression(user1);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result1Again.Should().BeTrue();
    }
}