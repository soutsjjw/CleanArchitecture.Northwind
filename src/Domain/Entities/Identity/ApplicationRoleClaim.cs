using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Domain.Entities.Identity;

/// <summary>
/// 擴充 IdentityRoleClaim，增加描述與群組資訊
/// </summary>
public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    /// <summary>
    /// EF Core 需要的無參數建構子
    /// </summary>
    public ApplicationRoleClaim() : base() { }

    /// <summary>
    /// 建構子：指定描述
    /// </summary>
    /// <param name="roleClaimDescription">角色聲明描述</param>
    public ApplicationRoleClaim(string roleClaimDescription)
        : this(roleClaimDescription, null) { }

    /// <summary>
    /// 建構子：指定描述與群組
    /// </summary>
    /// <param name="roleClaimDescription">角色聲明描述</param>
    /// <param name="roleClaimGroup">角色聲明群組</param>
    public ApplicationRoleClaim(string roleClaimDescription, string? roleClaimGroup) : base()
    {
        RoleClaimDescription = roleClaimDescription;
        RoleClaimGroup = roleClaimGroup;
    }

    /// <summary>
    /// 角色聲明描述
    /// </summary>
    public string? RoleClaimDescription { get; set; }

    /// <summary>
    /// 角色聲明群組
    /// </summary>
    public string? RoleClaimGroup { get; set; }

    public virtual ApplicationRole? Role { get; set; }
}
