using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Northwind.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsInRole(params string[] roleName)
    {
        var group = _httpContextAccessor.HttpContext?.User?.Claims?.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
        return group.Any(x => roleName.Contains(x));

    }
    public IEnumerable<string> GetRoles() => _httpContextAccessor.HttpContext?.User?.Claims?.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();


    public string DisplayName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.GivenName);
}
