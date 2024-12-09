using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Domain.Entities.Identity;

public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public string Description { get; set; }
    public string Group { get; set; }
    public virtual ApplicationRole Role { get; set; }

    public ApplicationRoleClaim() : base()
    {
    }

    public ApplicationRoleClaim(string roleClaimDescription = null, string roleClaimGroup = null) : base()
    {
        Description = roleClaimDescription;
        Group = roleClaimGroup;
    }
}
