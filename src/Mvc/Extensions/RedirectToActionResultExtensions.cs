using Microsoft.AspNetCore.Mvc;

namespace Mvc.Extensions;

public static class RedirectToActionResultExtensions
{
    public static RedirectToActionResult WithInfo(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithToast(controller, ToastType.Info, new[] { message }, title);

    public static RedirectToActionResult WithInfo(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
        => result.WithToast(controller, ToastType.Info, messages, title);

    public static RedirectToActionResult WithSuccess(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithToast(controller, ToastType.Success, new[] { message }, title);

    public static RedirectToActionResult WithSuccess(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
        => result.WithToast(controller, ToastType.Success, messages, title);

    public static RedirectToActionResult WithWarning(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithToast(controller, ToastType.Warning, new[] { message }, title);

    public static RedirectToActionResult WithWarning(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
        => result.WithToast(controller, ToastType.Warning, messages, title);

    public static RedirectToActionResult WithError(this RedirectToActionResult result, Controller controller, string message, string? title = null)
        => result.WithToast(controller, ToastType.Error, new[] { message }, title);

    public static RedirectToActionResult WithError(this RedirectToActionResult result, Controller controller, List<string> messages, string? title = null)
        => result.WithToast(controller, ToastType.Error, messages, title);

    private static RedirectToActionResult WithToast(this RedirectToActionResult result, Controller controller, ToastType type, IEnumerable<string>? messages, string? title = null)
    {
        var list = ToastExtensionsHelper.FilterMessages(messages);

        if (list is null || list.Count == 0)
            return result;

        ToastExtensionsHelper.AddToast(controller.TempData, type, list, title);
        return result;
    }
}
