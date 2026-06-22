namespace Notism.Application.Common.Services;

public interface ICacheInvalidator
{
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken);
}
