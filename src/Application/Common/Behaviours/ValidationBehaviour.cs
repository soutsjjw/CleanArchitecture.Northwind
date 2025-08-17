using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Common.Behaviours;
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(e => e is not null)
                .ToList();

            if (failures.Count > 0)
            {
                // 聚合成欄位 -> 訊息陣列
                var fieldErrors = failures
                    .GroupBy(f => f.PropertyName ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage)
                              .Where(m => !string.IsNullOrWhiteSpace(m))
                              .Distinct()
                              .ToArray(),
                        StringComparer.OrdinalIgnoreCase);

                var responseType = typeof(TResponse);

                // 1) 回傳 Result
                if (responseType == typeof(Result))
                {
                    return (TResponse)(object)Result.Invalid(fieldErrors, 400);
                }

                // 2) 回傳 Result<T>
                if (responseType.IsGenericType &&
                    responseType.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(Result<>).MakeGenericType(responseType.GetGenericArguments()[0]);

                    // 取用 Result<T>.Invalid(IDictionary<string,string[]>, int)
                    var invalidMethod = resultType.GetMethod(
                        "Invalid",
                        new[] { typeof(IDictionary<string, string[]>), typeof(int) });

                    if (invalidMethod is not null)
                    {
                        return (TResponse)invalidMethod.Invoke(null, new object[] { fieldErrors, 400 });
                    }
                }

                // 3) 其他 TResponse：維持原行為（丟出例外，由上層處理）
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}
