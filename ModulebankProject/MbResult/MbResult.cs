namespace ModulebankProject.MbResult;

public class MbResult<T, TE> where TE : Exception
{
    public bool IsSuccess;
    public T? Result { get; }
    public TE? Error { get; }

    private MbResult(T? result)
    {
        IsSuccess = true;
        Result = result;
        Error = null;
    }
    private MbResult(TE error)
    {
        IsSuccess = false;
        Result = default;
        Error = error;
    }

    public static MbResult<T, TE> Success(T result)
        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident Предлагает сделать нечитаемым
        => new MbResult<T, TE>(result);
    public static MbResult<T, TE> Failure(TE error)
        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident Предлагает сделать нечитаемым
        => new MbResult<T, TE>(error);

    public TResult Decide<TResult>(
        Func<T?, TResult> success,
        Func<TE?, TResult> failure)
    {
        return IsSuccess ? success(Result) : failure(Error);
    }
}