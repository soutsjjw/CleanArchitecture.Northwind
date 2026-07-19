using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole<string>, IAuditableEntity
{
    public ApplicationRole()
    {
        Id = Guid.NewGuid().ToString();
        RoleClaims = new HashSet<ApplicationRoleClaim>();
        UserRoles = new HashSet<ApplicationUserRole>();
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
        Id = Guid.NewGuid().ToString();
        RoleClaims = new HashSet<ApplicationRoleClaim>();
        UserRoles = new HashSet<ApplicationUserRole>();
    }

    public ApplicationRole(string roleName, int sort, string description) : base(roleName)
    {
        Id = Guid.NewGuid().ToString();
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
