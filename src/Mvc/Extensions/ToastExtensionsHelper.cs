using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Mvc.Extensions;

public static class ToastExtensionsHelper
{
    public static List<string>? FilterMessages(IEnumerable<string>? messages)
    {
        return messages?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    public static void AddToast(this ITempDataDictionary? tempData, ToastType type, List<string> messages, string? title = null)
    {
        if (tempData is null) return;

        var keyPrefix = $"Toast.{type}";
        tempData[$"{keyPrefix}.Messages"] = messages;

        if (!string.IsNullOrWhiteSpace(title))
            tempData[$"{keyPrefix}.Title"] = title;
    }
}
