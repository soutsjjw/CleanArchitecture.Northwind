using CleanArchitecture.Northwind.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Infrastructure.Identity;

public class ApplicationRole : IdentityRole, IAuditableEntity
{
    public ApplicationRole()
    {
        RoleClaims = new HashSet<ApplicationRoleClaim>();
        UserRoles = new HashSet<ApplicationUserRole>();
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
        RoleClaims = new HashSet<ApplicationRoleClaim>();
        UserRoles = new HashSet<ApplicationUserRole>();
    }

    public ApplicationRole(string roleName, int sort, string description) : base(roleName)
    {
        RoleClaims = new HashSet<ApplicationRoleClaim>();
        UserRoles = new HashSet<ApplicationUserRole>();

        Sort = sort;
        Description = description;
    }

    public int Sort { get; set; }
    public string? Description { get; set; }
    public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
