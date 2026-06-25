using Microsoft.AspNetCore.OutputCaching;

using Notism.Application.Common.Services;

namespace Notism.Api.Services;

public class OutputCacheInvalidator : ICacheInvalidator
{
    private readonly IOutputCacheStore _outputCacheStore;

    public OutputCacheInvalidator(IOutputCacheStore outputCacheStore)
        => _outputCacheStore = outputCacheStore;

    public async Task EvictByTagAsync(string tag, CancellationToken cancellationToken)
        => await _outputCacheStore.EvictByTagAsync(tag, cancellationToken);
}