using FluentAssertions;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;
using Notism.Application.Common.Services;
using Notism.Application.User.GetProfile;
using Notism.Domain.Common.Interfaces;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User.Enums;
using Notism.Shared.Exceptions;

using NSubstitute;

namespace Notism.Application.Tests.User.GetProfile;

public class GetUserProfileHandlerTests
{
    private readonly IRepository<Domain.User.User> _userRepository;
    private readonly IStorageService _storageService;
    private readonly ILogger<GetUserProfileHandler> _logger;
    private readonly IMessages _messages;
    private readonly GetUserProfileHandler _handler;

    public GetUserProfileHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<Domain.User.User>>();
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<GetUserProfileHandler>>();
        _messages = Substitute.For<IMessages>();

        _messages.UserNotFound.Returns("User not found.");

        _handler = new GetUserProfileHandler(
            _userRepository,
            _storageService,
            _logger,
            _messages);
    }

    [Fact]
    public async Task Handle_WhenUserExists_ReturnsProfileWithLocation()
    {
        var user = Domain.User.User.Create("test@example.com", "hashedpassword", UserRole.User, "John", "Doe");
        user.UpdateProfile("John", "Doe", null, "123 Main St, Hanoi");

        _userRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.User.User>>())
            .Returns(user);

        var request = new GetUserProfileRequest { UserId = user.Id };
        var result = await _handler.Handle(request, CancellationToken.None);

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

        _userRepository
            .FindByExpressionAsync(Arg.Any<FilterSpecification<Domain.User.User>>())
            .Returns(user);

        var request = new GetUserProfileRequest { UserId = user.Id };
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

        var request = new GetUserProfileRequest { UserId = Guid.NewGuid() };

        var act = async () => await _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<ResultFailureException>();
    }
}