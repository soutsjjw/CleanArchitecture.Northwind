using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("Region")]
public class Region : BaseEntity
{
    /// <summary>
    /// PK: Region.RegionID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("RegionID")]
    public int Id { get; set; }

    /// <summary>
    /// 區域描述，固定長度 50，不可為 null
    /// </summary>
    [Required]
    [Column("RegionDescription", TypeName = "nchar(50)")]
    public string RegionDescription { get; set; } = null!;

    /// <summary>
    /// 一個 Region 可對應多個 Territory
    /// </summary>
    public ICollection<Territory> Territories { get; private set; }
        = new List<Territory>();
}
