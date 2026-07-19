using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;

public interface IPermissionResolver
{
    /// <summary>
    /// 取得使用者目前具備的所有權限（合併角色與個人宣告）。
    /// </summary>
    Task<IReadOnlyList<Permission>> GetForUserAsync(
        ClaimsPrincipal user, CancellationToken ct = default);
}
