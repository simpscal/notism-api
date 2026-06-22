namespace Notism.Application.Common.Abstractions;

public interface ICacheInvalidator
{
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken);
}
