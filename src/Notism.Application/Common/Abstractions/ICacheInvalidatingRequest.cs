namespace Notism.Application.Common.Abstractions;

public interface ICacheInvalidatingRequest
{
    IEnumerable<string> CacheTagsToEvict { get; }
}
