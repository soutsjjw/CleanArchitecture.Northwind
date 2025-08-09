using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("Employees")]
public class Employee : BaseAuditableEntity<int>
{
    /// <summary>
    /// PK: Employees.EmployeeID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("EmployeeID")]
    public override int Id { get; set; }

    [Required, MaxLength(20)]
    public string LastName { get; set; } = null!;

    [Required, MaxLength(10)]
    public string FirstName { get; set; } = null!;

    [MaxLength(30)]
    public string? Title { get; set; }

    [MaxLength(25)]
    public string? TitleOfCourtesy { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime? HireDate { get; set; }

    [MaxLength(60)]
    public string? Address { get; set; }

    [MaxLength(15)]
    public string? City { get; set; }

    [MaxLength(15)]
    public string? Region { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(15)]
    public string? Country { get; set; }

    [MaxLength(24)]
    public string? HomePhone { get; set; }

    [MaxLength(4)]
    public string? Extension { get; set; }

    public byte[]? Photo { get; set; }

    [Required]
    public string Notes { get; set; } = null!;

    /// <summary>
    /// FK: Employees.EmployeeID (self-reference)
    /// </summary>
    public int? ReportsTo { get; set; }

    [ForeignKey(nameof(ReportsTo))]
    public Employee? Manager { get; set; }

    [InverseProperty(nameof(Manager))]
    public ICollection<Employee> DirectReports { get; private set; } = new List<Employee>();

    [MaxLength(255)]
    public string? PhotoPath { get; set; }

    public ICollection<EmployeeTerritory> EmployeeTerritories { get; private set; }
        = new List<EmployeeTerritory>();

    /// <summary>一個員工可有多筆訂單</summary>
    public ICollection<Order> Orders { get; private set; } = new List<Order>();
}
