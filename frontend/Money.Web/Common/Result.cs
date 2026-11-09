namespace Money.Web.Common;

public readonly struct Result
{
    private Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    public static Result Success()
    {
        return new(true, string.Empty);
    }

    public static Result Failure(string error)
    {
        return new(false, error);
    }

    public static Result<T> Success<T>(T value)
    {
        return Result<T>.Success(value);
    }

    public static Result<T> Failure<T>(string error)
    {
        return Result<T>.Failure(error);
    }
}

public readonly struct Result<T>
{
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public T Value { get; }

    public static Result<T> Success(T value)
    {
        return new(true, value, string.Empty);
    }

    public static Result<T> Failure(string error)
    {
        return new(false, default!, error);
    }

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> map)
    {
        return IsSuccess ? Result<TOut>.Success(map(Value)) : Result<TOut>.Failure(Error);
    }
}

public static class ResultExtensions
{
    public static async Task<Result<T>> Tap<T>(this Task<Result<T>> task, Action<T> action)
    {
        var result = await task;

        if (result.IsSuccess)
        {
            action(result.Value);
        }

        return result;
    }

    extension(Task<Result> task)
    {
        public async Task<Result> Tap(Action action)
        {
            var result = await task;

            if (result.IsSuccess)
            {
                action();
            }

            return result;
        }

        public async Task<Result> TapError(Action<string> action)
        {
            var result = await task;

            if (result.IsFailure)
            {
                action(result.Error);
            }

            return result;
        }

        public async Task<Result<T>> Map<T>(Func<T> map)
        {
            var result = await task;
            return result.IsSuccess ? Result<T>.Success(map()) : Result<T>.Failure(result.Error);
        }
    }
}
