using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

public class Customer : BaseAuditableEntity<string>
{
    /// <summary>
    /// PK: Customers.CustomerID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("CustomerID", TypeName = "nchar(5)")]
    public override string Id { get; set; } = null!;

    /// <summary>公司名稱，長度 40，不可為 null</summary>
    [Required]
    [MaxLength(40)]
    public string CompanyName { get; set; } = null!;

    /// <summary>聯絡人姓名，長度 30，可為 null</summary>
    [MaxLength(30)]
    public string? ContactName { get; set; }

    /// <summary>聯絡人職稱，長度 30，可為 null</summary>
    [MaxLength(30)]
    public string? ContactTitle { get; set; }

    /// <summary>地址，長度 60，可為 null</summary>
    [MaxLength(60)]
    public string? Address { get; set; }

    /// <summary>城市，長度 15，可為 null</summary>
    [MaxLength(15)]
    public string? City { get; set; }

    /// <summary>地區，長度 15，可為 null</summary>
    [MaxLength(15)]
    public string? Region { get; set; }

    /// <summary>郵遞區號，長度 10，可為 null</summary>
    [MaxLength(10)]
    public string? PostalCode { get; set; }

    /// <summary>國家，長度 15，可為 null</summary>
    [MaxLength(15)]
    public string? Country { get; set; }

    /// <summary>電話，長度 24，可為 null</summary>
    [MaxLength(24)]
    public string? Phone { get; set; }

    /// <summary>傳真，長度 24，可為 null</summary>
    [MaxLength(24)]
    public string? Fax { get; set; }

    /// <summary>一個客戶可有多筆訂單</summary>
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    public ICollection<CustomerCustomerDemo> CustomerCustomerDemos { get; private set; }
        = new List<CustomerCustomerDemo>();
}
