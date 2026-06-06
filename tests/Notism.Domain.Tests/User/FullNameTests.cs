using FluentAssertions;

using Notism.Domain.User.Enums;

namespace Notism.Domain.Tests.User;

public class FullNameTests
{
    [Fact]
    public void FullName_WhenBothNamesProvided_ReturnsTrimmedFullName()
    {
        var user = Domain.User.User.Create("a@b.com", "Password1!", UserRole.User, "John", "Doe");

        user.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void FullName_WhenBothNamesNull_ReturnsEmpty()
    {
        var user = Domain.User.User.Create("a@b.com", "Password1!", UserRole.User, null, null);

        user.FullName.Should().BeEmpty();
    }

    [Fact]
    public void FullName_WhenOnlyFirstNameProvided_ReturnsTrimmedFirstName()
    {
        var user = Domain.User.User.Create("a@b.com", "Password1!", UserRole.User, "John", null);

        user.FullName.Should().Be("John");
    }

    [Fact]
    public void FullName_WhenBothNamesWhitespace_ReturnsEmpty()
    {
        var user = Domain.User.User.Create("a@b.com", "Password1!", UserRole.User, "   ", "  ");

        user.FullName.Should().BeEmpty();
    }
}