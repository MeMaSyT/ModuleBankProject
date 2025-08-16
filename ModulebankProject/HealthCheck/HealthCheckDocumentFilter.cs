using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ModulebankProject.HealthCheck
{
    public class HealthCheckDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = new OpenApiPaths();

            paths.Add("/health/live", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { new() { Name = "HealthChecks" } },
                        Summary = "Liveness check",
                        Description = "Checks if the service is alive",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "Service is alive" }
                        }
                    }
                }
            });

            paths.Add("/health/ready", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { new() { Name = "HealthChecks" } },
                        Summary = "Readiness check",
                        Description = "Checks if the service is ready to accept requests",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "Service is ready" },
                            ["503"] = new OpenApiResponse { Description = "Service is not ready" }
                        }
                    }
                }
            });

            foreach (var path in paths)
            {
                swaggerDoc.Paths.Add(path.Key, path.Value);
            }
        }
    }
}
