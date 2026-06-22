using MediatR;

using Notism.Application.Common.Abstractions;

namespace Notism.Application.Common.Behaviors;

public class OutputCacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheInvalidator _cacheInvalidator;

    public OutputCacheInvalidationBehavior(ICacheInvalidator cacheInvalidator)
    {
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is ICacheInvalidatingRequest invalidatingRequest)
        {
            foreach (var tag in invalidatingRequest.CacheTagsToEvict)
            {
                await _cacheInvalidator.EvictByTagAsync(tag, cancellationToken);
            }
        }

        return response;
    }
}
