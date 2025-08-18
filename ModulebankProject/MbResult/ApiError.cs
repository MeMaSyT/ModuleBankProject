namespace ModulebankProject.MbResult;

public class ApiError : Exception
{
    public int StatusCode { get; }
    public object? Details { get; }

    // ReSharper disable once ConvertToPrimaryConstructor не хочу первичный конструктор
    public ApiError(string message, int statusCode, object? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public object GetResponse() => new
    {
        Message,
        StatusCode,
        Details
    };
}