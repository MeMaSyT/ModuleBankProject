using FluentValidation;
using ModulebankProject.MbResult;
using Newtonsoft.Json;

namespace ModulebankProject.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        // ReSharper disable once UnusedMember.Global используется в конвейере
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }


        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception.GetType() == typeof(ValidationException))
            {
                var errorMessage = "ValidationException: " + ((ValidationException)exception).Errors.FirstOrDefault()?.ErrorMessage;
                var error = new ApiError(errorMessage, StatusCodes.Status400BadRequest);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Success = false, Error = error.GetResponse()}));
            }
            else
            {
                var errorMessage = exception.Message;
                var error = new ApiError(errorMessage, StatusCodes.Status500InternalServerError);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Success = false, Error = error.GetResponse() }));
            }
        }
    }
}
