using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Extensions;

public static class ViewResultExtensions
{
    public static ViewResult WithInfo(this ViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Info, new[] { message }, title);

    public static ViewResult WithInfo(this ViewResult result, List<string> messages, string? title = null)
        => result.WithToast(ToastType.Info, messages, title);

    public static ViewResult WithSuccess(this ViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Success, new[] { message }, title);

    public static ViewResult WithSuccess(this ViewResult result, List<string> messages, string? title = null)
        => result.WithToast(ToastType.Success, messages, title);

    public static ViewResult WithWarning(this ViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Warning, new[] { message }, title);

    public static ViewResult WithWarning(this ViewResult result, List<string> messages, string? title = null)
        => result.WithToast(ToastType.Warning, messages, title);

    public static ViewResult WithError(this ViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Error, new[] { message }, title);

    public static ViewResult WithError(this ViewResult result, List<string> messages, string? title = null)
        => result.WithToast(ToastType.Error, messages, title);

    private static ViewResult WithToast(this ViewResult result, ToastType type, IEnumerable<string>? messages, string? title = null)
    {
        var list = ToastExtensionsHelper.FilterMessages(messages);

        if (list is null || list.Count == 0)
            return result;

        result.TempData.AddToast(type, list, title);
        return result;
    }
}
