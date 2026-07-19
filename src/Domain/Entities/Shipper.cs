using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("Shippers")]
public class Shipper : BaseEntity
{
    /// <summary>
    /// PK: Shippers.ShipperID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ShipperID")]
    public int Id { get; set; }

    /// <summary>承運商名稱，長度 40，不可為 null</summary>
    [Required]
    [MaxLength(40)]
    [Column("CompanyName")]
    public string CompanyName { get; set; } = null!;

    /// <summary>電話，長度 24，可為 null</summary>
    [MaxLength(24)]
    [Column("Phone")]
    public string? Phone { get; set; }

    /// <summary>一個 Shipper 可對應多筆訂單</summary>
    public ICollection<Order> Orders { get; private set; } = new List<Order>();
}
