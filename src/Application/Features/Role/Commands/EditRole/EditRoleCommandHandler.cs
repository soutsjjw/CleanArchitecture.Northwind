using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.EditRole;

public class EditRoleCommandHandler : IRequestHandler<EditRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<EditRoleCommandHandler> _logger;

    public EditRoleCommandHandler(IApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<EditRoleCommandHandler> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result> Handle(EditRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role == null)
        {
            return await Result.FailureAsync("未找到角色資料");
        }

        var allowPerms = new HashSet<(string Module, string Operation, DataScope? Scope)>();
        var opMaxScope = new Dictionary<string, DataScope>(); // key: "Module:Operation"

        void Consider(string module, string op, bool enabled, DataScope scope)
        {
            if (!enabled)
                return;

            if (op == nameof(DataScope.System))
            {
                allowPerms.Add((module, op, null));
                return;
            }
            allowPerms.Add((module, op, scope));
        }

        foreach (var r in request.Items)
        {
            Consider(r.Module, "Create", r.Create, r.Scope);
            Consider(r.Module, "Read", r.Read, r.Scope);
            Consider(r.Module, "Update", r.Update, r.Scope);
            Consider(r.Module, "Delete", r.Delete, r.Scope);
            Consider(r.Module, nameof(DataScope.System), r.System, r.Scope);
        }

        role.Name = request.Name;
        role.Description = request.Description;

        var updateRoleResult = await _roleManager.UpdateAsync(role);
        if (!updateRoleResult.Succeeded)
            throw new InvalidOperationException(string.Join("; ", updateRoleResult.Errors.Select(e => e.Description)));

        // 清掉舊的 permission / permission.scope（保留舊的 module級 scope 以維持相容或一併清掉皆可）
        var existing = await _roleManager.GetClaimsAsync(role);
        foreach (var c in existing.Where(c => c.Type is AppClaimTypes.Permission))
            await _roleManager.RemoveClaimAsync(role, c);

        foreach (var os in allowPerms.Distinct())
        {
            await _roleManager.AddClaimAsync(role, new Claim(AppClaimTypes.Permission, $"{os.Module}:{os.Operation}:{(os.Scope.HasValue ? os.Scope.Value.ToString() : "")}"));
        }

        return await Result.SuccessAsync("角色權限已成功更新");
    }
}
