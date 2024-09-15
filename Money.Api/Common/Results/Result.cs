namespace Money.Api.Common.Results;

public class ResultBase
{
    public bool IsFailure { get; protected init; }
    public bool IsSuccess => !IsFailure;

    public string[] Errors { get; protected init; } = [];
}

public class Result : ResultBase
{
    public static Result Success()
    {
        return new Result
        {
            IsFailure = false
        };
    }

    public static Result Failure(params string[] errors)
    {
        return new Result
        {
            IsFailure = true,
            Errors = errors
        };
    }
}

public class Result<T> : ResultBase
{
    public T? Data { get; private init; }

    public static implicit operator Result<T>(Result result)
    {
        return result.IsSuccess
            ? Success(default)
            : Failure(result.Errors);
    }

    public static implicit operator Result(Result<T> result)
    {
        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Errors);
    }

    public static Result<T> Success(T? data)
    {
        return new Result<T>
        {
            IsFailure = false,
            Data = data
        };
    }

    public static Result<T> Failure(params string[] errors)
    {
        return new Result<T>
        {
            IsFailure = true,
            Errors = errors
        };
    }
}
