using System.Diagnostics;
using System.Net;
using CleanArchitecture.Northwind.Application.Common.Exceptions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CleanArchitecture.Northwind.Infrastructure.Middleware;

public class ExceptionHandlerMiddleware : IMiddleware
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(
        ICurrentUserService currentUserService,
        ILogger<ExceptionHandlerMiddleware> logger
       )
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var errorId = await HandleExceptionAsync(context, exception);

            // 根據請求類型（API 或 MVC）返回不同的錯誤處理邏輯
            if (IsApiRequest(context.Request))
            {
                var responseModel = await HandleApiExceptionAsync(context, exception);
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(responseModel));
            }
            else
            {
                // 儲存錯誤訊息到 HttpContext.Items
                context.Items["ErrorId"] = errorId;
                context.Items["ErrorMessage"] = exception.Message;
                context.Items["StackTrace"] = exception.StackTrace;

                throw;
            }
        }
    }

    private async Task<string> HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var userId = _currentUserService.UserId;
        if (!string.IsNullOrEmpty(userId))
            LogContext.PushProperty("UserId", userId);

        var errorId = Activity.Current?.Id ?? context.TraceIdentifier;
        LogContext.PushProperty("ErrorId", errorId);
        LogContext.PushProperty("StackTrace", exception.StackTrace);

        var response = context.Response;
        var request = context.Request;
        response.ContentType = "application/json";

        // 預設狀態碼為 500
        response.StatusCode = (int)HttpStatusCode.InternalServerError;

        if (exception.InnerException != null)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
        }

        _logger.LogError(exception, $"Request failed with Status Code {response.StatusCode} and Error Id {errorId}.");

        return errorId;
    }

    private bool IsApiRequest(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/api") ||
               request.Headers["Accept"].Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<object> HandleApiExceptionAsync(HttpContext context, Exception exception)
    {
        // API 的 HttpCode 統一紀錄在 Result 中的 StatusCode
        context.Response.StatusCode = 200;

        return exception switch
        {
            ValidationException e => await Result.FailureAsync(e.Errors.Select(x => $"{x.Key}:{string.Join(',', x.Value)}"),
                (int)HttpStatusCode.BadRequest),
            NotFoundException e => await Result.FailureAsync(new[] { e.Message },
                (int)HttpStatusCode.NotFound),
            KeyNotFoundException e => await Result.FailureAsync(new[] { e.Message },
                (int)HttpStatusCode.NotFound),
            UnauthorizedException e => await Result.FailureAsync(new[] { e.Message },
                (int)HttpStatusCode.Unauthorized),
            _ => await Result.FailureAsync(new[] { exception.Message },
                (int)HttpStatusCode.InternalServerError)
        };
    }
}
