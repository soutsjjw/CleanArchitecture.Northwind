using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Infrastructure.Identity;

public class ApplicationUserClaim : IdentityUserClaim<string>
{
    public string? Description { get; set; }
    public virtual ApplicationUser User { get; set; } = default!;
}
