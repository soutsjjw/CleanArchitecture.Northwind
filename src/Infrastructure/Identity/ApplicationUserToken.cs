using CleanArchitecture.Northwind.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class ApplicationUserToken : IdentityUserToken<string>
{
    public virtual ApplicationUser User { get; set; } = default!;
}
