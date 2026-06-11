using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Tests.Common;
using Notism.Application.User.GetProfile;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.User.GetProfile;

/// <summary>
/// Exercises the <see cref="GetUserByIdQuery"/> behind <see cref="GetUserProfileHandler"/>
/// against an EF InMemory database: the by-id lookup and location mapping.
/// </summary>
public class GetUserProfileHandlerTests
{
    private readonly AppDbContext _dbContext;
    private readonly IMessages _messages;
    private readonly GetUserProfileHandler _handler;

    public GetUserProfileHandlerTests()
    {
        _dbContext = ReadDbContextFactory.Create();
        _messages = Substitute.For<IMessages>();
        _messages.UserNotFound.Returns("User not found.");

        _handler = new GetUserProfileHandler(
            _dbContext,
            Substitute.For<IStorageService>(),
            Substitute.For<ILogger<GetUserProfileHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsProfileWithLocation()
    {
        var user = Domain.User.User.Create("test@example.com", "hashedpassword", UserRole.User, "John", "Doe");
        user.UpdateProfile("John", "Doe", null, "123 Main St, Hanoi");
        await SeedAsync(user);

        var result = await _handler.Handle(new GetUserProfileRequest { UserId = user.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Location.Should().Be("123 Main St, Hanoi");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoLocation_ReturnsNullLocation()
    {
        var user = Domain.User.User.Create("test@example.com", "hashedpassword", UserRole.User, "Jane", "Smith");
        await SeedAsync(user);

        var result = await _handler.Handle(new GetUserProfileRequest { UserId = user.Id }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Location.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsResultFailureException()
    {
        var act = async () => await _handler.Handle(
            new GetUserProfileRequest { UserId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    private async Task SeedAsync(Domain.User.User user)
    {
        user.ClearDomainEvents();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
    }
}
