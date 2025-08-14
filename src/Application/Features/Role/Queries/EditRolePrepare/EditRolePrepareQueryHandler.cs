using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.EditRolePrepare;

public class EditRolePrepareQueryHandler : IRequestHandler<EditRolePrepareQuery, Result<EditRolePrepareDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<EditRolePrepareQueryHandler> _logger;

    public EditRolePrepareQueryHandler(IApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<EditRolePrepareQueryHandler> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result<EditRolePrepareDto>> Handle(EditRolePrepareQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId);
        if (role is null)
        {
            _logger.LogError($"找不到角色 ID: {request.RoleId}");
            return await Result<EditRolePrepareDto>.FailureAsync("找不到角色資料");
        }

        // 讀 claims
        //var (perms, permScopesByOp) = await GetRoleOpMatrixAsync(role);
        var perms = await GetRoleOpMatrixAsync(role);

        var items = new List<EditRoleItemPrepareDto>();
        foreach (var module in Modules.All)
        {
            foreach (var scope in new[] { DataScope.Self, DataScope.Office, DataScope.Department, DataScope.All, DataScope.System })
            {
                // 若此 module/op 的最大範圍 >= 本列範圍，顯示為已勾
                bool Checked(string op)
                {
                    if (op == nameof(DataScope.System))
                    {
                        return perms.Contains($"{module}:{op}:");
                    }
                    return perms.Contains($"{module}:{op}:{scope}");
                }

                items.Add(new EditRoleItemPrepareDto
                {
                    Module = module,
                    Scope = scope,
                    Create = Checked("Create"),
                    Read = Checked("Read"),
                    Update = Checked("Update"),
                    Delete = Checked("Delete"),
                    System = Checked(nameof(DataScope.System))
                });
            }
        }

        var editRolePrepareDto = new EditRolePrepareDto
        {
            Name = role.Name ?? "",
            Description = role.Description ?? "",
            RoleId = role.Id,
            RoleName = role.Name!,
            Items = items
        };

        return await Result<EditRolePrepareDto>.SuccessAsync(editRolePrepareDto);
    }

    //private async Task<(HashSet<string> perms, Dictionary<string, DataScope> permScopes)> GetRoleOpMatrixAsync(ApplicationRole? role)
    //{
    //    var claims = await _roleManager.GetClaimsAsync(role);

    //    var perms = claims.Where(c => c.Type == AppClaimTypes.Permission)
    //                      .Select(c => c.Value)
    //                      .ToHashSet();

    //    var permScopes = new Dictionary<string, DataScope>(); // key: "Module:Operation" → Max scope
    //    foreach (var c in claims.Where(c => c.Type == AppClaimTypes.PermissionScope))
    //    {
    //        var parts = c.Value.Split(':'); // Module:Operation:ScopeInt
    //        if (parts.Length == 3 && int.TryParse(parts[2], out var s))
    //        {
    //            var key = $"{parts[0]}:{parts[1]}";
    //            var scope = (DataScope)s;
    //            if (!permScopes.TryGetValue(key, out var cur) || scope > cur)
    //                permScopes[key] = scope;
    //        }
    //    }
    //    return (perms, permScopes);
    //}

    private async Task<HashSet<string>> GetRoleOpMatrixAsync(ApplicationRole role)
    {
        var claims = await _roleManager.GetClaimsAsync(role);

        var perms = claims.Where(c => c.Type == AppClaimTypes.Permission)
                          .Select(c => c.Value)
                          .ToHashSet();

        return perms;
    }
}
