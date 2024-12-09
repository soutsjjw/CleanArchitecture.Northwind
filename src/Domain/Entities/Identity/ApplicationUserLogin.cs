using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Domain.Entities.Identity;

public class ApplicationUserLogin : IdentityUserLogin<string>
{
    public virtual ApplicationUser User { get; set; } = default!;
}
