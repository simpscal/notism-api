using FluentAssertions;

using MediatR;

using Notism.Application.Common.Abstractions;
using Notism.Application.Common.Behaviors;

using NSubstitute;

namespace Notism.Application.Tests.Common;

public class OutputCacheInvalidationBehaviorTests
{
    private readonly ICacheInvalidator _cacheInvalidator = Substitute.For<ICacheInvalidator>();

    private record InvalidatingRequest(IEnumerable<string> CacheTagsToEvict)
        : IRequest<string>, ICacheInvalidatingRequest;

    private record NonInvalidatingRequest : IRequest<string>;

    [Fact]
    public async Task Handle_WhenRequestInvalidatesCache_EvictsEachDeclaredTag()
    {
        var behavior = new OutputCacheInvalidationBehavior<InvalidatingRequest, string>(_cacheInvalidator);
        var request = new InvalidatingRequest(["categories", "foods"]);

        var response = await behavior.Handle(
            request,
            () => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
        await _cacheInvalidator.Received(1).EvictByTagAsync("categories", Arg.Any<CancellationToken>());
        await _cacheInvalidator.Received(1).EvictByTagAsync("foods", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRequestDoesNotInvalidateCache_DoesNotEvict()
    {
        var behavior = new OutputCacheInvalidationBehavior<NonInvalidatingRequest, string>(_cacheInvalidator);

        var response = await behavior.Handle(
            new NonInvalidatingRequest(),
            () => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
        await _cacheInvalidator.DidNotReceiveWithAnyArgs().EvictByTagAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WhenHandlerThrows_DoesNotEvict()
    {
        var behavior = new OutputCacheInvalidationBehavior<InvalidatingRequest, string>(_cacheInvalidator);
        var request = new InvalidatingRequest(["foods"]);

        var act = async () => await behavior.Handle(
            request,
            () => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _cacheInvalidator.DidNotReceiveWithAnyArgs().EvictByTagAsync(default!, default);
    }
}
