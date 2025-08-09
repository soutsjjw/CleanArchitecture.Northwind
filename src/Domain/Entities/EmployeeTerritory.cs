using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("EmployeeTerritories")]
public class EmployeeTerritory
{
    /// <summary>
    /// PK1 + FK → Employees.EmployeeID
    /// </summary>
    [Key]
    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    /// <summary>
    /// PK2 + FK → Territories.TerritoryID
    /// </summary>
    [Column("TerritoryID", TypeName = "nvarchar(20)")]
    public string TerritoryId { get; set; } = null!;

    /// <summary>
    /// 多對一：多筆 EmployeeTerritory 屬於一位 Employee
    /// </summary>
    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    /// <summary>
    /// 多對一：多筆 EmployeeTerritory 屬於一個 Territory
    /// </summary>
    [ForeignKey(nameof(TerritoryId))]
    public Territory Territory { get; set; } = null!;
}
