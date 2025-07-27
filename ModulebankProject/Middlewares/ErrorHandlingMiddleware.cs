using FluentValidation;
using System.Net;
using Newtonsoft.Json;

namespace ModulebankProject.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

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


        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception.GetType() == typeof(ValidationException))
            {
                var code = HttpStatusCode.BadRequest;
                var result = JsonConvert.SerializeObject(((ValidationException)exception).Errors);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                return context.Response.WriteAsync(result);
            }
            else
            {
                var code = HttpStatusCode.InternalServerError;
                var result = JsonConvert.SerializeObject(new { isSuccess = false, error = exception.Message });
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                return context.Response.WriteAsync(result);
            }
        }
    }
}
