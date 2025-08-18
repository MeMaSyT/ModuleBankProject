using System.Diagnostics;

namespace ModulebankProject.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        // Input
        _logger.LogInformation("HTTP request started {@RequestInfo}", new
        {
            RequestId = Guid.NewGuid(),
            CorrelationId = correlationId,
            context.Request.Method,
            context.Request.Path,
            QueryString = context.Request.QueryString.Value,
            Headers = context.Request.Headers
                .Where(h => !h.Key.StartsWith("Authorization"))
                .ToDictionary(h => h.Key, h => h.Value.ToString())
        });

        try
        {
            await _next(context);

            // Output
            _logger.LogInformation("HTTP request completed {@ResponseInfo}", new
            {
                RequestId = Guid.NewGuid(),
                CorrelationId = correlationId,
                context.Response.StatusCode,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                context.Response.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request failed {@ErrorInfo}", new
            {
                RequestId = Guid.NewGuid(),
                CorrelationId = correlationId,
                LatencyMs = stopwatch.ElapsedMilliseconds
            });
            throw;
        }
    }
}