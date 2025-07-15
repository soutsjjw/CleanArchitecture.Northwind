using Microsoft.AspNetCore.Mvc;

namespace Mvc.ViewComponents;

public class ToastViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var infoMessages = GetMessages("Toast.Info.Messages");
        var infoTitle = GetMessages("Toast.Info.Title");
        var successMessages = GetMessages("Toast.Success.Messages");
        var successTitle = GetMessages("Toast.Success.Title");
        var warningMessages = GetMessages("Toast.Warning.Messages");
        var warningTitle = GetMessages("Toast.Warning.Title");
        var errorMessages = GetMessages("Toast.Error.Messages");
        var errorTitle = GetMessages("Toast.Error.Title");

        ViewData["InfoMessages"] = infoMessages;
        ViewData["InfoTitle"] = infoTitle;
        ViewData["SuccessMessages"] = successMessages;
        ViewData["SuccessTitle"] = successTitle;
        ViewData["WarningMessages"] = warningMessages;
        ViewData["WarningTitle"] = warningTitle;
        ViewData["ErrorMessages"] = errorMessages;
        ViewData["ErrorTitle"] = errorTitle;

        return View();
    }

    List<string>? GetMessages(string key)
    {
        return TempData[key] as List<string>
            ?? (TempData[key] as string[])?.ToList();
    }
}
