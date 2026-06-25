namespace Notism.Application.Common.Behaviors;

public interface ICacheInvalidatingRequest
{
    IEnumerable<string> CacheTagsToEvict { get; }
}