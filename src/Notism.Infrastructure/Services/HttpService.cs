using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;

namespace Notism.Infrastructure.Services;

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpService> _logger;

    public HttpService(HttpClient httpClient, ILogger<HttpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T> PostAsync<T>(
        string url,
        Dictionary<string, string> formData,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var requestContent = new FormUrlEncodedContent(formData);
        var response = await _httpClient.PostAsync(url, requestContent, cancellationToken);

        return await HandleResponseAsync<T>(response, errorMessage ?? "HTTP POST request failed", cancellationToken);
    }

    public async Task<T> GetAsync<T>(
        string url,
        string? bearerToken = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        return await HandleResponseAsync<T>(response, errorMessage ?? "HTTP GET request failed", cancellationToken);
    }

    public async Task<T> PostJsonAsync<T>(
        string url,
        object? jsonBody = null,
        string? bearerToken = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var jsonContent = jsonBody != null
            ? JsonSerializer.Serialize(jsonBody)
            : string.Empty;

        var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = requestContent,
        };

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);

        return await HandleResponseAsync<T>(response, errorMessage ?? "HTTP POST request failed", cancellationToken);
    }

    private async Task<T> HandleResponseAsync<T>(
        HttpResponseMessage response,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var requestUrl = response.RequestMessage?.RequestUri?.ToString() ?? "Unknown URL";

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var statusCode = (int)response.StatusCode;

            _logger.LogError(
                "External API call failed. URL: {Url}, StatusCode: {StatusCode}, ErrorMessage: {ErrorMessage}, ResponseContent: {ResponseContent}",
                requestUrl,
                statusCode,
                errorMessage,
                errorContent);

            throw new Exception();
        }

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            var deserialized = JsonSerializer.Deserialize<T>(jsonContent);

            if (deserialized == null)
            {
                _logger.LogError(
                    "Failed to deserialize external API response. URL: {Url}, ResponseContent: {ResponseContent}",
                    requestUrl,
                    jsonContent);

                throw new Exception();
            }

            return deserialized;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(
                jsonEx,
                "JSON deserialization error for external API response. URL: {Url}, ResponseContent: {ResponseContent}",
                requestUrl,
                jsonContent);

            throw new Exception();
        }
    }
}

