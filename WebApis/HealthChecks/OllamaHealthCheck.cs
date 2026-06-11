using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApis.HealthChecks;

public sealed class OllamaHealthCheck(IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpClient client = httpClientFactory.CreateClient("Ollama");

            HttpResponseMessage response =
                await client.GetAsync(
                    "/api/tags",
                    cancellationToken);

            if (response.IsSuccessStatusCode)
                return HealthCheckResult.Healthy("Ollama is running.");

            return HealthCheckResult.Unhealthy($"Ollama returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Ollama unavailable.", ex);
        }
    }
}