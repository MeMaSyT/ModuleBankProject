using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ModulebankProject.HealthCheck;

public class BasicHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        return Task.FromResult(
            HealthCheckResult.Healthy("Application is running"));
    }
}