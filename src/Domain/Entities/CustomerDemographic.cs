using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

[Table("CustomerDemographics")]
public class CustomerDemographic : BaseAuditableEntity<string>
{
    /// <summary>
    /// PK: CustomerDemographics.CustomerTypeID (nchar(10))
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("CustomerTypeID", TypeName = "nchar(10)")]
    public override string Id { get; set; } = null!;

    /// <summary>
    /// 類型描述，可為 null
    /// </summary>
    [Column("CustomerDesc", TypeName = "ntext")]
    public string? CustomerDesc { get; set; }

    /// <summary>
    /// 反向導航：一個 CustomerDemographic 可對應多筆 CustomerCustomerDemo
    /// </summary>
    public ICollection<CustomerCustomerDemo> CustomerCustomerDemos { get; private set; }
        = new List<CustomerCustomerDemo>();
}
