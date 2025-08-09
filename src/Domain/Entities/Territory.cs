using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("Territories")]
public class Territory : BaseAuditableEntity<string>
{
    /// <summary>
    /// PK: Territories.TerritoryID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("TerritoryID", TypeName = "nvarchar(20)")]
    public override string Id { get; set; } = null!;

    /// <summary>
    /// 區域描述，固定長度 50，不可為 null
    /// </summary>
    [Required]
    [Column("TerritoryDescription", TypeName = "nchar(50)")]
    public string TerritoryDescription { get; set; } = null!;

    /// <summary>
    /// FK: Territories.RegionID
    /// </summary>
    [Column("RegionID")]
    public int RegionId { get; set; }

    /// <summary>
    /// 多對一：多筆 Territory 屬於一個 Region
    /// </summary>
    [ForeignKey(nameof(RegionId))]
    public Region Region { get; set; } = null!;

    public ICollection<EmployeeTerritory> EmployeeTerritories { get; private set; }
        = new List<EmployeeTerritory>();
}
