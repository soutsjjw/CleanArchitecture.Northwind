using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("Suppliers")]
public class Supplier : BaseAuditableEntity<int>
{
    /// <summary>
    /// PK: Suppliers.SupplierID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("SupplierID")]
    public override int Id { get; set; }

    /// <summary>公司名稱，長度 40，不可為 null</summary>
    [Required]
    [MaxLength(40)]
    [Column("CompanyName")]
    public string CompanyName { get; set; } = null!;

    /// <summary>聯絡人姓名，長度 30，可為 null</summary>
    [MaxLength(30)]
    [Column("ContactName")]
    public string? ContactName { get; set; }

    /// <summary>聯絡人職稱，長度 30，可為 null</summary>
    [MaxLength(30)]
    [Column("ContactTitle")]
    public string? ContactTitle { get; set; }

    /// <summary>地址，長度 60，可為 null</summary>
    [MaxLength(60)]
    [Column("Address")]
    public string? Address { get; set; }

    /// <summary>城市，長度 15，可為 null</summary>
    [MaxLength(15)]
    [Column("City")]
    public string? City { get; set; }

    /// <summary>地區，長度 15，可為 null</summary>
    [MaxLength(15)]
    [Column("Region")]
    public string? Region { get; set; }

    /// <summary>郵遞區號，長度 10，可為 null</summary>
    [MaxLength(10)]
    [Column("PostalCode")]
    public string? PostalCode { get; set; }

    /// <summary>國家，長度 15，可為 null</summary>
    [MaxLength(15)]
    [Column("Country")]
    public string? Country { get; set; }

    /// <summary>電話，長度 24，可為 null</summary>
    [MaxLength(24)]
    [Column("Phone")]
    public string? Phone { get; set; }

    /// <summary>傳真，長度 24，可為 null</summary>
    [MaxLength(24)]
    [Column("Fax")]
    public string? Fax { get; set; }

    /// <summary>網站首頁（超連結），可為 null</summary>
    [Column("HomePage", TypeName = "ntext")]
    public string? HomePage { get; set; }

    /// <summary>反向導航：一個 Supplier 可對應多筆 Products</summary>
    public ICollection<Product> Products { get; private set; } = new List<Product>();
}
