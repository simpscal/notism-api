using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.Tests.Common;
using Notism.Application.User.UpdateProfile;
using Notism.Domain.User.Enums;
using Notism.Infrastructure.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.User.UpdateProfile;

public class UpdateUserProfileHandlerTests : IDisposable
{
    private readonly WriteHandlerContext _context;
    private readonly IMessages _messages;
    private readonly UpdateUserProfileHandler _handler;

    public UpdateUserProfileHandlerTests()
    {
        _context = new WriteHandlerContext();
        _messages = Substitute.For<IMessages>();
        _messages.UserNotFound.Returns("User not found.");

        var userRepository = new UserRepository(_context.DbContext, Substitute.For<IPasswordService>());

        _handler = new UpdateUserProfileHandler(
            userRepository,
            _context.DbContext,
            Substitute.For<ILogger<UpdateUserProfileHandler>>(),
            _messages);
    }

    [Fact]
    public async Task Handle_WhenLocationProvided_UpdatesLocationAndReturnsIt()
    {
        var user = Domain.User.User.Create("test@example.com", "hashedpassword", UserRole.User, "John", "Doe");
        await _context.SeedAsync(user);

        var request = new UpdateUserProfileRequest
        {
            UserId = user.Id,
            FirstName = "John",
            LastName = "Doe",
            Location = "456 Nguyen Hue, Ho Chi Minh City",
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Location.Should().Be("456 Nguyen Hue, Ho Chi Minh City");

        _context.DbContext.ChangeTracker.Clear();
        _context.DbContext.Users.Single(u => u.Id == user.Id).Location
            .Should().Be("456 Nguyen Hue, Ho Chi Minh City");
    }

    [Fact]
    public async Task Handle_WhenLocationIsNull_ClearsLocation()
    {
        var user = Domain.User.User.Create("test@example.com", "hashedpassword", UserRole.User, "John", "Doe");
        user.UpdateProfile("John", "Doe", null, "Old address");
        await _context.SeedAsync(user);

        var request = new UpdateUserProfileRequest
        {
            UserId = user.Id,
            FirstName = "John",
            LastName = "Doe",
            Location = null,
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Location.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsResultFailureException()
    {
        var request = new UpdateUserProfileRequest
        {
            UserId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }

    public void Dispose()
        => _context.Dispose();
}