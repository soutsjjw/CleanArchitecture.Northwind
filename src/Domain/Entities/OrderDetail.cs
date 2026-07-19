using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("OrderDetails")]
public class OrderDetail
{
    /// <summary>
    /// 複合主鍵之一 + FK → Orders.OrderID
    /// </summary>
    [Key]
    [Column("OrderID")]
    public int OrderId { get; set; }

    /// <summary>
    /// 複合主鍵之一 + FK → Products.ProductID
    /// </summary>
    [Column("ProductID")]
    public int ProductId { get; set; }

    /// <summary>單價</summary>
    [Column("UnitPrice", TypeName = "money")]
    public decimal UnitPrice { get; set; }

    /// <summary>訂購數量</summary>
    [Column("Quantity")]
    public short Quantity { get; set; }

    /// <summary>折扣（0–1 之間）</summary>
    [Column("Discount")]
    public float Discount { get; set; }

    /// <summary>訂單導覽</summary>
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    /// <summary>產品導覽</summary>
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
}
