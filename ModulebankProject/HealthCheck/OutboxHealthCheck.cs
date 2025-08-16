using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ModulebankProject.Infrastructure.Data;

namespace ModulebankProject.HealthCheck
{
    public class OutboxHealthCheck : IHealthCheck
    {
        private readonly ModulebankDataContext _modulebankDataContext;
        private readonly ILogger<OutboxHealthCheck> _logger;

        public OutboxHealthCheck(ModulebankDataContext modulebankDataContext, ILogger<OutboxHealthCheck> logger)
        {
            _modulebankDataContext = modulebankDataContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var pendingCount = await _modulebankDataContext.OutboxMessages
                    .Where(x => x.ProcessedOn != null)
                    .CountAsync(cancellationToken: cancellationToken);

                if (pendingCount == 0)
                {
                    return HealthCheckResult.Healthy("No pending messages in Outbox");
                }

                var status = pendingCount switch
                {
                    < 100 => HealthStatus.Healthy,
                    < 500 => HealthStatus.Degraded,
                    _ => HealthStatus.Unhealthy
                };

                var message = $"Pending Outbox messages: {pendingCount}";

                if (pendingCount > 100)
                {
                    _logger.LogWarning(message);
                    return HealthCheckResult.Degraded(message);
                }

                return new HealthCheckResult(status, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox health check failed");
                return HealthCheckResult.Unhealthy("Outbox check failed", ex);
            }
        }
    }
}
