using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("Orders")]
public class Order : BaseAuditableEntity<int>, IOwnedResource
{
    /// <summary>
    /// PK: Orders.OrderID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("OrderID")]
    public override int Id { get; set; }

    /// <summary>
    /// FK → Customers.CustomerID
    /// </summary>
    [Column("CustomerID", TypeName = "nchar(5)")]
    public string? CustomerId { get; set; }

    public int DepartmentId { get; set; }

    public int OfficeId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    /// <summary>
    /// FK → Employees.EmployeeID
    /// </summary>
    [Column("EmployeeID")]
    public int? EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [Column("OrderDate")]
    public DateTime? OrderDate { get; set; }

    [Column("RequiredDate")]
    public DateTime? RequiredDate { get; set; }

    [Column("ShippedDate")]
    public DateTime? ShippedDate { get; set; }

    /// <summary>
    /// FK → Shippers.ShipperID
    /// </summary>
    [Column("ShipVia")]
    public int? ShipVia { get; set; }

    [ForeignKey(nameof(ShipVia))]
    public Shipper? Shipper { get; set; }

    [Column("Freight", TypeName = "money")]
    public decimal? Freight { get; set; }

    [MaxLength(40)]
    [Column("ShipName")]
    public string? ShipName { get; set; }

    [MaxLength(60)]
    [Column("ShipAddress")]
    public string? ShipAddress { get; set; }

    [MaxLength(15)]
    [Column("ShipCity")]
    public string? ShipCity { get; set; }

    [MaxLength(15)]
    [Column("ShipRegion")]
    public string? ShipRegion { get; set; }

    [MaxLength(10)]
    [Column("ShipPostalCode")]
    public string? ShipPostalCode { get; set; }

    [MaxLength(15)]
    [Column("ShipCountry")]
    public string? ShipCountry { get; set; }

    /// <summary>
    /// 反向導航：一筆訂單可有多筆明細
    /// </summary>
    public ICollection<OrderDetail> OrderDetails { get; private set; }
        = new List<OrderDetail>();
}
