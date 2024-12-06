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

            var userId = _currentUserService.UserId;
            if (!string.IsNullOrEmpty(userId)) LogContext.PushProperty("UserId", userId);
            string errorId = Guid.NewGuid().ToString();
            LogContext.PushProperty("ErrorId", errorId);
            LogContext.PushProperty("StackTrace", exception.StackTrace);
            var responseModel = await Result.FailureAsync(new string[] { exception.Message });
            var response = context.Response;
            response.ContentType = "application/json";
            if (exception.InnerException != null)
            {
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
            }

            response.StatusCode = 200;

            switch (exception)
            {
                case ValidationException e:
                    responseModel = await Result.FailureAsync(e.Errors.Select(x => $"{x.Key}:{string.Join(',', x.Value)}"),
                        (int)HttpStatusCode.BadRequest);
                    break;
                case NotFoundException e:
                    responseModel = await Result.FailureAsync(new string[] { e.Message },
                        (int)HttpStatusCode.NotFound);
                    break;
                case KeyNotFoundException e:
                    responseModel = await Result.FailureAsync(new string[] { e.Message },
                        (int)HttpStatusCode.NotFound);
                    break;
                case UnauthorizedException e:
                    responseModel = await Result.FailureAsync(new string[] { e.Message },
                        (int)HttpStatusCode.Unauthorized);
                    break;
                default:
                    responseModel = await Result.FailureAsync(new string[] { exception.Message },
                        (int)HttpStatusCode.InternalServerError);
                    break;
            }

            _logger.LogError(exception, $"Request failed with Status Code {response.StatusCode} and Error Id {errorId}.");

            await response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(responseModel));
        }
    }
}
