using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("Products")]
public class Product : BaseAuditableEntity<int>
{
    /// <summary>
    /// PK: Products.ProductID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ProductID")]
    public override int Id { get; set; }

    /// <summary>產品名稱，長度 40，不可為 null</summary>
    [Required]
    [MaxLength(40)]
    [Column("ProductName")]
    public string ProductName { get; set; } = null!;

    /// <summary>FK → Suppliers.SupplierID</summary>
    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    /// <summary>FK → Categories.CategoryID</summary>
    [Column("CategoryID")]
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    /// <summary>單位包裝 (例: ’10 boxes x 20 bags’)，長度 20，可為 null</summary>
    [MaxLength(20)]
    [Column("QuantityPerUnit")]
    public string? QuantityPerUnit { get; set; }

    /// <summary>單價</summary>
    [Column("UnitPrice", TypeName = "money")]
    public decimal? UnitPrice { get; set; }

    /// <summary>庫存數量</summary>
    [Column("UnitsInStock")]
    public short? UnitsInStock { get; set; }

    /// <summary>在訂購中數量</summary>
    [Column("UnitsOnOrder")]
    public short? UnitsOnOrder { get; set; }

    /// <summary>再訂購水準</summary>
    [Column("ReorderLevel")]
    public short? ReorderLevel { get; set; }

    /// <summary>是否已中止 (0 = 在售, 1 = 中止)</summary>
    [Column("Discontinued")]
    public bool Discontinued { get; set; }

    /// <summary>一個產品可出現在多筆訂單明細</summary>
    public ICollection<OrderDetail> OrderDetails { get; private set; }
        = new List<OrderDetail>();
}
