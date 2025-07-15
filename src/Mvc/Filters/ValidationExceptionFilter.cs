using CleanArchitecture.Northwind.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Mvc.Extensions;

namespace Mvc.Filters;

public class ValidationExceptionFilter : IAsyncExceptionFilter
{
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    public ValidationExceptionFilter(
        IModelMetadataProvider modelMetadataProvider,
        ITempDataDictionaryFactory tempDataDictionaryFactory)
    {
        _modelMetadataProvider = modelMetadataProvider;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        // 只處理 FluentValidation 拋出的 ValidationException
        if (context.Exception is ValidationException ex)
        {
            // 把每個 failure 加到 ModelState
            foreach (var failure in ex.Errors)
                foreach (var err in failure.Value)
                    context.ModelState.AddModelError(failure.Key, err);

            // 嘗試從 HttpContext.Items 取出 action arguments
            var items = context.HttpContext.Items;
            var args = items["__ActionArguments__"]
                       as IDictionary<string, object>;
            var modelCommand = args?.Values.FirstOrDefault();

            var req = context.HttpContext.Request;
            var isApiOrAjax = req.Headers["Accept"].ToString().Contains("application/json")
                              || req.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isApiOrAjax)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
            else
            {
                var actionName = context.RouteData.Values["action"]?.ToString();
                var vdata = new ViewDataDictionary(_modelMetadataProvider, context.ModelState)
                {
                    Model = modelCommand
                };
                var tdata = _tempDataDictionaryFactory.GetTempData(context.HttpContext);

                var result = (new ViewResult
                {
                    ViewName = actionName,
                    ViewData = vdata,
                    TempData = tdata
                });

                if (!context.ModelState.IsValid)
                {
                    result.WithError(context.ModelState
                        .Values
                        .SelectMany(entry => entry.Errors)
                        .Select(err => err.ErrorMessage)
                        .Where(msg => !string.IsNullOrEmpty(msg))
                        .ToList());
                }

                context.Result = result;
            }

            context.ExceptionHandled = true;
        }

        return Task.CompletedTask;
    }
}
