namespace CommonMethodLibrary.Application.Common;

/// <summary>
/// 操作结果包装类 - 统一的操作结果返回
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public string[]? Errors { get; }

    private Result(bool isSuccess, T? data, string? error, string[]? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Errors = errors;
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> Failure(string[] errors) => new(false, default, null, errors);
}

/// <summary>
/// 无返回值的操作结果
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string[]? Errors { get; }

    private Result(bool isSuccess, string? error, string[]? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(string[] errors) => new(false, null, errors);
}
