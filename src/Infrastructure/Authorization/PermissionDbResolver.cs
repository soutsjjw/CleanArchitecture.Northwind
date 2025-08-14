using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Northwind.Infrastructure.Authorization;

public sealed class PermissionDbResolver : IPermissionResolver
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly PermissionResolverOptions _opt;

    public PermissionDbResolver(
        ApplicationDbContext db,
        IMemoryCache cache,
        IOptions<PermissionResolverOptions> opt)
    {
        _db = db;
        _cache = cache;
        _opt = opt.Value;
    }

    public async Task<IReadOnlyList<Permission>> GetForUserAsync(
        ClaimsPrincipal user, CancellationToken ct = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirst("sub")?.Value
                  ?? user.FindFirst("uid")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Array.Empty<Permission>();

        var cacheKey = $"perm:{userId}";

        // 快取命中
        if (_opt.CacheSeconds > 0 &&
            _cache.TryGetValue(cacheKey, out IReadOnlyList<Permission> cached))
        {
            return cached;
        }

        // 1) 取使用者角色 Id
        var roleIds = await _db.Set<ApplicationUserRole>()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        // 2) 角色權限宣告（AspNetRoleClaims）
        var roleClaims = await _db.Set<ApplicationRoleClaim>()
            .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == _opt.ClaimType)
            .Select(rc => rc.ClaimValue!)
            .ToListAsync(ct);

        // 3) 個人權限宣告（AspNetUserClaims）
        var userClaims = await _db.Set<ApplicationUserClaim>()
            .Where(uc => uc.UserId == userId && uc.ClaimType == _opt.ClaimType)
            .Select(uc => uc.ClaimValue!)
            .ToListAsync(ct);

        // 4) 合併、去重、解析
        var values = roleClaims
            .Concat(userClaims)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var list = new List<Permission>(capacity: 32);
        foreach (var v in values)
        {
            if (PermissionParser.TryParse(v, out var p))
                list.Add(p);
        }

        var result = (IReadOnlyList<Permission>)list;

        // 5) 寫入快取
        if (_opt.CacheSeconds > 0)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromSeconds(_opt.CacheSeconds));
        }

        return result;
    }
}
