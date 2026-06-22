namespace Notism.Application.Common.Caching;

public interface ICacheInvalidatingRequest
{
    IEnumerable<string> CacheTagsToEvict { get; }
}
