using CleanArchitecture.Northwind.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual ApplicationUserProfile Profile { get; set; }

    public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    public virtual ICollection<ApplicationUserLogin> Logins { get; set; }
    public virtual ICollection<ApplicationUserToken> Tokens { get; set; }
}
