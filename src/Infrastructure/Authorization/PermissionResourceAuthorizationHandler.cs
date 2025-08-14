using System.Security.Claims;
using CleanArchitecture.Northwind.Domain.Common;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Northwind.Infrastructure.Authorization;

// <summary>
/// 帶「資源」的授權處理器：先確認有該 Module:Operation 的 permission，
/// 再以使用者的最大允許資料範圍（permission.scope > scope）對照資源的實際範圍。
/// Admin 角色萬能直通。
/// </summary>
public sealed class PermissionResourceAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement req,
        IOwnedResource? resource)
    {
        if (resource is null)
            return Task.CompletedTask;

        // 1) Admin 直通
        if (context.User.IsInRole(Roles.SystemAdmin) || context.User.IsInRole(Roles.Administrator))
        {
            context.Succeed(req);
            return Task.CompletedTask;
        }

        //var opKey = $"{req.Module}:{req.Operation}";
        var opSystemKey = $"{req.Required.Module}:{nameof(DataScope.System)}:";
        var opAllKey = $"{req.Required.Module}:{req.Required.Operation}:{nameof(DataScope.All)}";

        var opKey = $"{req.Required.Module}:{req.Required.Operation}:{req.Required.Scope}";

        if (context.User.HasClaim(AppClaimTypes.Permission, opSystemKey)
            || context.User.HasClaim(AppClaimTypes.Permission, opAllKey))
        {
            context.Succeed(req);
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirstValue(AppClaimTypes.UserId);
        var officeId = context.User.FindFirstValue(AppClaimTypes.OfficeId)
                       ?? context.User.FindFirst(nameof(AppClaimTypes.OfficeId))?.Value;
        var deptId = context.User.FindFirstValue(AppClaimTypes.DepartmentId);

        var resourceLevel = GetResourceLevel(resource, userId, officeId, deptId);
        var allowed = GetMaxAllowedScope(context.User, req.Required.Module, req.Required.Operation);

        if (allowed >= resourceLevel)
            context.Succeed(req);

        return Task.CompletedTask;
    }

    private static DataScope GetResourceLevel(
        IOwnedResource r, string? userId, string? officeId, string? deptId)
    {
        if (!string.IsNullOrEmpty(userId) && r.CreatedBy == userId) return DataScope.Self;
        if (!string.IsNullOrEmpty(officeId) && r.OfficeId.ToString() == officeId) return DataScope.Office;
        if (!string.IsNullOrEmpty(deptId) && r.DepartmentId.ToString() == deptId) return DataScope.Department;
        return DataScope.All;
    }

    private static DataScope GetMaxAllowedScope(ClaimsPrincipal user, string module, string operation)
    {
        var opMax = user.FindAll(AppClaimTypes.Permission)
            .Select(c => c.Value.Split(':'))
            .Where(a => a.Length == 3 && a[0] == module && a[1] == operation)
            .Select(a => TryParseScope(a[2], out var s) ? s : (DataScope?)null)
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .DefaultIfEmpty()
            .Cast<DataScope?>()
            .Max();

        if (opMax.HasValue)
            return opMax.Value;

        return DataScope.None;
    }

    private static bool TryParseScope(string value, out DataScope scope)
    {
        if (int.TryParse(value, out var i) && Enum.IsDefined(typeof(DataScope), i))
        {
            scope = (DataScope)i;
            return true;
        }
        if (Enum.TryParse<DataScope>(value, ignoreCase: true, out var parsed))
        {
            scope = parsed;
            return true;
        }
        scope = default;
        return false;
    }
}
