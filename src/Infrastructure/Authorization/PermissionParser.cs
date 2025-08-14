using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Infrastructure.Authorization;

public static class PermissionParser
{
    public static bool TryParse(string text, out Permission permission)
    {
        permission = default;

        if (string.IsNullOrWhiteSpace(text)) return false;

        // 期望格式：Module:Action:Scope
        var parts = text.Trim().Split(':');
        if (parts.Length != 3) return false;

        var module = parts[0];
        var operation = parts[1];
        if (!Enum.TryParse(parts[2], ignoreCase: true, out DataScope scope))
        {
            scope = DataScope.None;
        }

        permission = new Permission(module, operation, scope);
        return true;
    }
}
