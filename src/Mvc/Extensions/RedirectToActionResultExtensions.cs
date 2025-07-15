using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Mvc.Extensions;

public static class RedirectToActionResultExtensions
{
    public static RedirectToActionResult WithInfo(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithInfo(controller, new List<string> { message }, title);

    public static RedirectToActionResult WithInfo(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        controller.TempData?.AddToast("Info", messages, title);
        return result;
    }

    public static RedirectToActionResult WithSuccess(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithSuccess(controller, new List<string> { message }, title);

    public static RedirectToActionResult WithSuccess(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        controller.TempData?.AddToast("Success", messages, title);
        return result;
    }

    public static RedirectToActionResult WithWarning(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithWarning(controller, new List<string> { message }, title);

    public static RedirectToActionResult WithWarning(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        controller.TempData?.AddToast("Warning", messages, title);
        return result;
    }

    public static RedirectToActionResult WithError(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithError(controller, new List<string> { message }, title);

    public static RedirectToActionResult WithError(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        controller.TempData?.AddToast("Error", messages, title);
        return result;
    }

    private static void AddToast(this ITempDataDictionary? tempData, string type, List<string> messages, string? title = null)
    {
        if (tempData == null) return;
        tempData[$"Toast.{type}.Messages"] = messages;
        if (!string.IsNullOrEmpty(title))
            tempData[$"Toast.{type}.Title"] = title;
    }
}
