using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CleanArchitecture.Northwind.Infrastructure.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        // 取得 enum 欄位資訊
        var member = value.GetType()
                          .GetMember(value.ToString())
                          .FirstOrDefault();
        if (member != null)
        {
            // 取 DisplayAttribute
            var displayAttr = member
                .GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null)
                return displayAttr.Name;
        }
        // fallback 為 enum 原始字串
        return value.ToString();
    }
}
