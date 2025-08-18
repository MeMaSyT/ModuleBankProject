using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ModulebankProject.HealthCheck;

public class HealthCheckDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = new OpenApiPaths
        {
            {
                "/health/live", new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [OperationType.Get] = new()
                        {
                            Tags = new List<OpenApiTag> { new() { Name = "HealthChecks" } },
                            Summary = "Liveliness check",
                            Description = "Checks if the service is alive",
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new() { Description = "Service is alive" }
                            }
                        }
                    }
                }
            },
            {
                "/health/ready", new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [OperationType.Get] = new()
                        {
                            Tags = new List<OpenApiTag> { new() { Name = "HealthChecks" } },
                            Summary = "Readiness check",
                            Description = "Checks if the service is ready to accept requests",
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new() { Description = "Service is ready" },
                                ["503"] = new() { Description = "Service is not ready" }
                            }
                        }
                    }
                }
            }
        };

        foreach (var path in paths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}