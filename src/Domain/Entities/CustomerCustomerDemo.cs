using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("CustomerCustomerDemo")]
public class CustomerCustomerDemo : BaseAuditableEntity
{
    /// <summary>
    /// FK + PK1: Customers.CustomerID
    /// </summary>
    [Key]  // 主鍵／外鍵之一，複合主鍵由 Fluent API 設定
    [Column("CustomerID", TypeName = "nchar(5)")]
    public string CustomerId { get; set; } = null!;

    /// <summary>
    /// PK2 + FK: CustomerDemographics.CustomerTypeID
    /// </summary>
    [Column("CustomerTypeID", TypeName = "nchar(10)")]
    public string CustomerTypeId { get; set; } = null!;

    /// <summary>外鍵導覽：多筆 CustomerCustomerDemo 屬於一個 Customer</summary>
    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    /// <summary>外鍵導覽：多筆 CustomerCustomerDemo 屬於一個 CustomerDemographic</summary>
    [ForeignKey(nameof(CustomerTypeId))]
    public CustomerDemographic CustomerDemographic { get; set; } = null!;
}
