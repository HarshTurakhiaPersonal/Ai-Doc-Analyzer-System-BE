using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApis.HealthChecks;

public sealed class StorageHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            string uploadPath = configuration["Storage:UploadRoot"]!;

            if (!Directory.Exists(uploadPath))
                return Task.FromResult(HealthCheckResult.Unhealthy("Upload directory does not exist."));

            return Task.FromResult(HealthCheckResult.Healthy("Storage available."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Storage unavailable.", ex));
        }
    }
}