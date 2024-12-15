using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mvc.ViewModels;

namespace Mvc.Controllers;
public class ErrorController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        var iExceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var showStackTrace = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        var errorId = HttpContext.Items["ErrorId"] as string;
        var errorMessage = HttpContext.Items["ErrorMessage"] as string;
        var stackTrace = HttpContext.Items["StackTrace"] as string;

        var viewModel = new ErrorViewModel
        {
            ErrorId = errorId,
            ShowStackTrace = showStackTrace,
            ErrorMessage = errorMessage,
            StackTrace = stackTrace
        };

        if (string.IsNullOrEmpty(viewModel.ErrorId) && iExceptionHandlerFeature != null)
        {
            viewModel.ErrorId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            viewModel.ErrorMessage = iExceptionHandlerFeature.Error.Message;
            viewModel.StackTrace = iExceptionHandlerFeature.Error.StackTrace;
        }

        return View(viewModel);
    }

    public IActionResult PageNotFound()
    {
        return View();
    }
}
