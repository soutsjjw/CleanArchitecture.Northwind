using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Extensions;

public static class PartialViewResultExtensions
{
    public static PartialViewResult WithInfo(this PartialViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Info, new[] { message }, title);

    public static PartialViewResult WithInfo(this PartialViewResult result, IEnumerable<string> messages, string? title = null)
        => result.WithToast(ToastType.Info, messages, title);

    public static PartialViewResult WithSuccess(this PartialViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Success, new[] { message }, title);

    public static PartialViewResult WithSuccess(this PartialViewResult result, IEnumerable<string> messages, string? title = null)
        => result.WithToast(ToastType.Success, messages, title);

    public static PartialViewResult WithWarning(this PartialViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Warning, new[] { message }, title);

    public static PartialViewResult WithWarning(this PartialViewResult result, IEnumerable<string> messages, string? title = null)
        => result.WithToast(ToastType.Warning, messages, title);


    public static PartialViewResult WithError(this PartialViewResult result, string message, string? title = null)
        => result.WithToast(ToastType.Error, new[] { message }, title);

    public static PartialViewResult WithError(this PartialViewResult result, List<string> messages, string? title = null)
        => result.WithToast(ToastType.Error, messages, title);

    private static PartialViewResult WithToast(this PartialViewResult result, ToastType type, IEnumerable<string>? messages, string? title = null)
    {
        var list = ToastExtensionsHelper.FilterMessages(messages);

        if (list is null || list.Count == 0)
            return result;

        result.TempData.AddToast(type, list, title);
        return result;
    }
}
