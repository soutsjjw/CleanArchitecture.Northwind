using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Application.Common.Models;

public class Result : IResult
{
    internal Result()
    {

    }

    internal Result(bool succeeded, IEnumerable<string> errors, int statusCode = 200)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        StatusCode = statusCode;
    }

    public bool Succeeded { get; set; }
    public string[] Errors { get; set; }
    public string[] Messages { get; set; }
    public int StatusCode { get; set; }

    #region 同步

    public static Result Success(int statusCode = 200)
    {
        return new Result { Succeeded = true, StatusCode = statusCode };
    }

    public static Result Success(string message, int statusCode = 200)
    {
        return new Result { Succeeded = true, Messages = new string[] { message }, StatusCode = statusCode };
    }

    public static Result Success(IEnumerable<string> messages, int statusCode = 200)
    {
        return new Result { Succeeded = true, Messages = messages.ToArray(), StatusCode = statusCode };
    }

    public static Result Failure(string error, int statusCode = 500)
    {
        return new Result { Succeeded = false, Errors = new string[] { error }, StatusCode = statusCode };
    }

    public static Result Failure(IEnumerable<string> errors, int statusCode = 500)
    {
        return new Result { Succeeded = false, Errors = errors.ToArray(), StatusCode = statusCode };
    }

    #endregion

    #region 非同步

    public static Task<Result> SuccessAsync(int statusCode = 200)
    {
        return Task.FromResult(Success(statusCode));
    }
    public static Task<Result> SuccessAsync(string message, int statusCode = 200)
    {
        return Task.FromResult(Success(message, statusCode));
    }
    public static Task<Result> SuccessAsync(IEnumerable<string> messages, int statusCode = 200)
    {
        return Task.FromResult(Success(messages, statusCode));
    }
    public static Task<Result> FailureAsync(string error, int statusCode = 500)
    {
        return Task.FromResult(Failure(error, statusCode));
    }

    public static Task<Result> FailureAsync(IEnumerable<string> errors, int statusCode = 500)
    {
        return Task.FromResult(Failure(errors, statusCode));
    }

    #endregion
}

public class Result<T> : Result, IResult<T>
{
    public T Data { get; set; }

    #region 同步

    public static Result<T> Success(T data, int statusCode = 200)
    {
        return new Result<T> { Succeeded = true, Data = data, StatusCode = statusCode };
    }

    public static Result<T> Success(T data, string message, int statusCode = 200)
    {
        return new Result<T> { Succeeded = true, Data = data, Messages = new string[] { message }, StatusCode = statusCode };
    }

    public static Result<T> Success(T data, IEnumerable<string> messages, int statusCode = 200)
    {
        return new Result<T> { Succeeded = true, Data = data, Messages = messages.ToArray(), StatusCode = statusCode };
    }

    public static new Result<T> Failure(string error, int statusCode = 500)
    {
        return new Result<T> { Succeeded = false, Errors = new string[] { error }, StatusCode = statusCode };
    }

    public static new Result<T> Failure(IEnumerable<string> errors, int statusCode = 500)
    {
        return new Result<T> { Succeeded = false, Errors = errors.ToArray(), StatusCode = statusCode };
    }

    #endregion

    #region 非同步

    public static async Task<Result<T>> SuccessAsync(T data, int statusCode = 200)
    {
        return await Task.FromResult(Success(data, statusCode));
    }

    public static async Task<Result<T>> SuccessAsync(T data, string message, int statusCode = 200)
    {
        return await Task.FromResult(Success(data, message, statusCode));
    }

    public static async Task<Result<T>> SuccessAsync(T data, IEnumerable<string> messages, int statusCode = 200)
    {
        return await Task.FromResult(Success(data, messages, statusCode));
    }

    public static new async Task<Result<T>> FailureAsync(string error, int statusCode = 500)
    {
        return await Task.FromResult(Failure(error, statusCode));
    }

    public static new async Task<Result<T>> FailureAsync(IEnumerable<string> errors, int statusCode = 500)
    {
        return await Task.FromResult(Failure(errors, statusCode));
    }

    #endregion
}
