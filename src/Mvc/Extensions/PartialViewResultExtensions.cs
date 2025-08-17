using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Mvc.Extensions;


public static class PartialViewResultExtensions
{
    public static PartialViewResult WithInfo(this PartialViewResult result, string message, string? title = null)
    {
        return WithInfo(result, new List<string> { message }, title);
    }

    public static PartialViewResult WithInfo(this PartialViewResult result, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        result.TempData?.AddToast("Info", messages, title);
        return result;
    }

    public static PartialViewResult WithSuccess(this PartialViewResult result, string message, string? title = null)
    {
        return WithSuccess(result, new List<string> { message }, title);
    }

    public static PartialViewResult WithSuccess(this PartialViewResult result, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        result.TempData?.AddToast("Success", messages, title);
        return result;
    }

    public static PartialViewResult WithWarning(this PartialViewResult result, string message, string? title = null)
    {
        return WithWarning(result, new List<string> { message }, title);
    }

    public static PartialViewResult WithWarning(this PartialViewResult result, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        result.TempData?.AddToast("Warning", messages, title);
        return result;
    }

    public static PartialViewResult WithError(this PartialViewResult result, string message, string? title = null)
    {
        return WithError(result, new List<string> { message }, title);
    }

    public static PartialViewResult WithError(this PartialViewResult result, List<string> messages, string? title = null)
    {
        if (messages == null || messages.Count == 0)
            return result;

        result.TempData?.AddToast("Error", messages, title);
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
