using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Services;
using Notism.Application.User.UpdateProfile;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User;
using Notism.Domain.User.Enums;
using Notism.Domain.User.Repositories;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.User.UpdateProfile;

public class UpdateUserProfileHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserProfileHandler> _logger;
    private readonly IMessages _messages;
    private readonly UpdateUserProfileHandler _handler;

    public UpdateUserProfileHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UpdateUserProfileHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.UserNotFound.Returns("User not found.");

        _handler = new UpdateUserProfileHandler(
            _userRepository,
            _logger,
            _messages);
    }

    [Fact]
    public async Task Handle_WhenLocationProvided_UpdatesLocationAndReturnsIt()
    {
        var user = Domain.User.User.Create("test@example.com", "hashedpassword", UserRole.User, "John", "Doe");

        _userRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.User.User>>())
            .Returns(user);

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
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenLocationIsNull_ClearsLocation()
    {
        var user = Domain.User.User.Create("test@example.com", "hashedpassword", UserRole.User, "John", "Doe");
        user.UpdateProfile("John", "Doe", null, "Old address");

        _userRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.User.User>>())
            .Returns(user);

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
        _userRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.User.User>>())
            .Returns((Domain.User.User?)null);

        var request = new UpdateUserProfileRequest
        {
            UserId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
        };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }
}