using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Application.Common.Models;

public class Result : IResult
{
    internal Result() { }

    internal Result(bool succeeded, IEnumerable<string> errors, int statusCode = 200)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        StatusCode = statusCode;
    }

    public bool Succeeded { get; set; }

    public string[] _Errors = Array.Empty<string>();

    public string[] Errors
    {
        get
        {
            if (FieldErrors.Any())
            {
                _Errors = Array.Empty<string>();

                FieldErrors.Select(fieldError => fieldError.Value)
                    .Where(values => values != null)
                    .SelectMany(values => values)
                    .ToList()
                    .ForEach(value => _Errors = _Errors.Append(value).ToArray());
            }
            return _Errors;
        }
        set
        {
            _Errors = value ?? Array.Empty<string>();
            if (_Errors.Length > 0)
            {
                Succeeded = false; // 如果有錯誤，則設定 Succeeded 為 false
            }
        }
    }
    public string[] Messages { get; set; } = Array.Empty<string>();
    public int StatusCode { get; set; } = 200;

    // === 新增：欄位驗證錯誤 ===
    public Dictionary<string, string[]> FieldErrors { get; set; }
        = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    public bool HasFieldErrors => FieldErrors.Count > 0;

    #region 同步

    public static Result Success(int statusCode = 200)
        => new() { Succeeded = true, StatusCode = statusCode };

    public static Result Success(string message, int statusCode = 200)
        => new() { Succeeded = true, Messages = new[] { message }, StatusCode = statusCode };

    public static Result Success(IEnumerable<string> messages, int statusCode = 200)
        => new() { Succeeded = true, Messages = messages.ToArray(), StatusCode = statusCode };

    public static Result Failure(string error, int statusCode = 500)
        => new() { Succeeded = false, Errors = new[] { error }, StatusCode = statusCode };

    public static Result Failure(IEnumerable<string> errors, int statusCode = 500)
        => new() { Succeeded = false, Errors = errors.ToArray(), StatusCode = statusCode };

    public static Result Invalid(IDictionary<string, string> fieldErrors, int statusCode = 400)
        => new()
        {
            Succeeded = false,
            StatusCode = statusCode,
            FieldErrors = fieldErrors.ToDictionary(
                kv => kv.Key,
                kv => new[] { kv.Value },
                StringComparer.OrdinalIgnoreCase)
        };

    public static Result Invalid(IDictionary<string, string[]> fieldErrors, int statusCode = 400)
        => new()
        {
            Succeeded = false,
            StatusCode = statusCode,
            FieldErrors = new Dictionary<string, string[]>(fieldErrors, StringComparer.OrdinalIgnoreCase)
        };

    public static Result Invalid(
        IDictionary<string, string[]> fieldErrors,
        IEnumerable<string>? errors,
        IEnumerable<string>? messages = null,
        int statusCode = 400)
        => new()
        {
            Succeeded = false,
            StatusCode = statusCode,
            Errors = errors?.ToArray() ?? Array.Empty<string>(),
            Messages = messages?.ToArray() ?? Array.Empty<string>(),
            FieldErrors = new Dictionary<string, string[]>(fieldErrors, StringComparer.OrdinalIgnoreCase)
        };

    #endregion

    #region 非同步

    public static Task<Result> SuccessAsync(int statusCode = 200)
        => Task.FromResult(Success(statusCode));

    public static Task<Result> SuccessAsync(string message, int statusCode = 200)
        => Task.FromResult(Success(message, statusCode));

    public static Task<Result> SuccessAsync(IEnumerable<string> messages, int statusCode = 200)
        => Task.FromResult(Success(messages, statusCode));

    public static Task<Result> FailureAsync(string error, int statusCode = 500)
        => Task.FromResult(Failure(error, statusCode));

    public static Task<Result> FailureAsync(IEnumerable<string> errors, int statusCode = 500)
        => Task.FromResult(Failure(errors, statusCode));

    public static Task<Result> InvalidAsync(IDictionary<string, string> fieldErrors, int statusCode = 400)
        => Task.FromResult(Invalid(fieldErrors, statusCode));

    public static Task<Result> InvalidAsync(IDictionary<string, string[]> fieldErrors, int statusCode = 400)
        => Task.FromResult(Invalid(fieldErrors, statusCode));

    public static Task<Result> InvalidAsync(
        IDictionary<string, string[]> fieldErrors,
        IEnumerable<string>? errors,
        IEnumerable<string>? messages = null,
        int statusCode = 400)
        => Task.FromResult(Invalid(fieldErrors, errors, messages, statusCode));

    #endregion
}

public class Result<T> : Result, IResult<T>
{
    public T Data { get; set; }

    #region 同步

    public static Result<T> Success(T data, int statusCode = 200)
        => new() { Succeeded = true, Data = data, StatusCode = statusCode };

    public static Result<T> Success(T data, string message, int statusCode = 200)
        => new() { Succeeded = true, Data = data, Messages = new[] { message }, StatusCode = statusCode };

    public static Result<T> Success(T data, IEnumerable<string> messages, int statusCode = 200)
        => new() { Succeeded = true, Data = data, Messages = messages.ToArray(), StatusCode = statusCode };

    public static new Result<T> Failure(string error, int statusCode = 500)
        => new() { Succeeded = false, Errors = new[] { error }, StatusCode = statusCode };

    public static new Result<T> Failure(IEnumerable<string> errors, int statusCode = 500)
        => new() { Succeeded = false, Errors = errors.ToArray(), StatusCode = statusCode };

    public static Result<T> Invalid(IDictionary<string, string> fieldErrors, int statusCode = 400)
        => new()
        {
            Succeeded = false,
            StatusCode = statusCode,
            FieldErrors = fieldErrors.ToDictionary(
                kv => kv.Key,
                kv => new[] { kv.Value },
                StringComparer.OrdinalIgnoreCase)
        };

    public static Result<T> Invalid(IDictionary<string, string[]> fieldErrors, int statusCode = 400)
        => new()
        {
            Succeeded = false,
            StatusCode = statusCode,
            FieldErrors = new Dictionary<string, string[]>(fieldErrors, StringComparer.OrdinalIgnoreCase)
        };

    public static Result<T> Invalid(
        IDictionary<string, string[]> fieldErrors,
        IEnumerable<string>? errors,
        IEnumerable<string>? messages = null,
        int statusCode = 400)
        => new()
        {
            Succeeded = false,
            StatusCode = statusCode,
            Errors = errors?.ToArray() ?? Array.Empty<string>(),
            Messages = messages?.ToArray() ?? Array.Empty<string>(),
            FieldErrors = new Dictionary<string, string[]>(fieldErrors, StringComparer.OrdinalIgnoreCase)
        };

    #endregion

    #region 非同步

    public static Task<Result<T>> SuccessAsync(T data, int statusCode = 200)
        => Task.FromResult(Success(data, statusCode));

    public static Task<Result<T>> SuccessAsync(T data, string message, int statusCode = 200)
        => Task.FromResult(Success(data, message, statusCode));

    public static Task<Result<T>> SuccessAsync(T data, IEnumerable<string> messages, int statusCode = 200)
        => Task.FromResult(Success(data, messages, statusCode));

    public static new Task<Result<T>> FailureAsync(string error, int statusCode = 500)
        => Task.FromResult(Failure(error, statusCode));

    public static new Task<Result<T>> FailureAsync(IEnumerable<string> errors, int statusCode = 500)
        => Task.FromResult(Failure(errors, statusCode));

    // === 新增：Invalid 的非同步版本 ===
    public static Task<Result<T>> InvalidAsync(IDictionary<string, string> fieldErrors, int statusCode = 400)
        => Task.FromResult(Invalid(fieldErrors, statusCode));

    public static Task<Result<T>> InvalidAsync(IDictionary<string, string[]> fieldErrors, int statusCode = 400)
        => Task.FromResult(Invalid(fieldErrors, statusCode));

    public static Task<Result<T>> InvalidAsync(
        IDictionary<string, string[]> fieldErrors,
        IEnumerable<string>? errors,
        IEnumerable<string>? messages = null,
        int statusCode = 400)
        => Task.FromResult(Invalid(fieldErrors, errors, messages, statusCode));

    #endregion
}
