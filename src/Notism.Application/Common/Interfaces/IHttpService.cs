namespace Notism.Application.Common.Interfaces;

public interface IHttpService
{
    Task<T> PostAsync<T>(
        string url,
        Dictionary<string, string> formData,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    Task<T> GetAsync<T>(
        string url,
        string? bearerToken = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    Task<T> PostJsonAsync<T>(
        string url,
        object? jsonBody = null,
        string? bearerToken = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);
}

