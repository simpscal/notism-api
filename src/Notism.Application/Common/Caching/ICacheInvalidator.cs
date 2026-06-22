namespace Notism.Application.Common.Caching;

public interface ICacheInvalidator
{
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken);
}
