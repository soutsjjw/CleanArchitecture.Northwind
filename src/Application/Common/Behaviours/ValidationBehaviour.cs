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

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v =>
                    v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                // 若 TResponse 是 Result 或 Result<T>
                var errorMessages = failures.Select(e => e.ErrorMessage).ToArray();

                // 處理 Result<T>
                var responseType = typeof(TResponse);
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(Result<>).MakeGenericType(responseType.GetGenericArguments()[0]);
                    var failureMethod = resultType.GetMethod("Failure", new[] { typeof(IEnumerable<string>), typeof(int) });
                    return (TResponse)failureMethod.Invoke(null, new object[] { errorMessages, 400 });
                }
                // 處理 Result
                if (responseType == typeof(Result))
                {
                    return (TResponse)(object)Result.Failure(errorMessages, 400);
                }

                // 若不是 Result 型別，仍可選擇丟例外或回傳預設值
                throw new ValidationException(failures);
                //return default!;
            }
        }
        return await next();
    }
}
