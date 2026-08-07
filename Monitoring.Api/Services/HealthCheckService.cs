using System.Diagnostics;

namespace Monitoring.Api.Services;

/// <summary>
/// Service for performing HTTP health checks on target URLs
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Performs a health check on a target URL and records the response
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <param name="cancellationToken">Cancellation token for the HTTP request</param>
    /// <returns>Health check result with response time, status code, and health status</returns>
    Task<HealthCheckResult> CheckHealthAsync(string url, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a single health check
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>
    /// HTTP status code received (0 if connection failed)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Whether the target is healthy (status 200-299)
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Error message if the check failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the check was successful (regardless of target health)
    /// </summary>
    public bool IsSuccessful => ErrorMessage == null;
}

public class HealthCheckService : IHealthCheckService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthCheckService> _logger;

    // Timeout for HTTP requests in seconds
    private const int RequestTimeoutSeconds = 10;

    public HealthCheckService(HttpClient httpClient, ILogger<HealthCheckService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Performs an HTTP health check on the specified URL
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("Health check attempted with empty URL");
            return new HealthCheckResult
            {
                StatusCode = 0,
                IsHealthy = false,
                ErrorMessage = "URL is empty or null",
                ResponseTimeMs = 0
            };
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Performing health check for URL: {Url}", url);

            // Create a request with timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(RequestTimeoutSeconds));

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            stopwatch.Stop();

            var responseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            var statusCode = (int)response.StatusCode;
            var isHealthy = statusCode >= 200 && statusCode <= 299;

            _logger.LogInformation(
                "Health check completed for URL: {Url} | StatusCode: {StatusCode} | ResponseTime: {ResponseTimeMs}ms | Healthy: {IsHealthy}",
                url,
                statusCode,
                responseTimeMs,
                isHealthy
            );

            return new HealthCheckResult
            {
                StatusCode = statusCode,
                IsHealthy = isHealthy,
                ResponseTimeMs = responseTimeMs,
                ErrorMessage = null
            };
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                ex,
                "HTTP request exception during health check for URL: {Url} | Message: {Message}",
                url,
                ex.Message
            );

            return new HealthCheckResult
            {
                StatusCode = 0,
                IsHealthy = false,
                ErrorMessage = $"HTTP request failed: {ex.Message}",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(
                ex,
                "Health check timeout for URL: {Url} after {TimeoutSeconds} seconds",
                url,
                RequestTimeoutSeconds
            );

            return new HealthCheckResult
            {
                StatusCode = 0,
                IsHealthy = false,
                ErrorMessage = $"Request timeout after {RequestTimeoutSeconds} seconds",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Unexpected error during health check for URL: {Url} | Message: {Message}",
                url,
                ex.Message
            );

            return new HealthCheckResult
            {
                StatusCode = 0,
                IsHealthy = false,
                ErrorMessage = $"Unexpected error: {ex.Message}",
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }
}
