using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.ViewComponents;

public class ToastViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        // 從 TempData 讀取各類型的訊息
        var infoMessages = GetMessages("Toast.Info.Messages");
        var infoTitle = GetMessages("Toast.Info.Title");
        var successMessages = GetMessages("Toast.Success.Messages");
        var successTitle = GetMessages("Toast.Success.Title");
        var warningMessages = GetMessages("Toast.Warning.Messages");
        var warningTitle = GetMessages("Toast.Warning.Title");
        var errorMessages = GetMessages("Toast.Error.Messages");
        var errorTitle = GetMessages("Toast.Error.Title");

        // 傳遞給視圖
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
