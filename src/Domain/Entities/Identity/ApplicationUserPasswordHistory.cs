using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities.Identity;

public class ApplicationUserPasswordHistory
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime ChangedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }
}
