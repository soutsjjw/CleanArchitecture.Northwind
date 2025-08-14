using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

public class Category : BaseEntity
{
    /// <summary>
    /// PK 對應到 Categories.CategoryID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("CategoryID")]
    public int Id { get; set; }

    /// <summary>
    /// 類別名稱，最大長度 15，不可為 null
    /// </summary>
    [Required]
    [MaxLength(15)]
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// 類別描述，可為 null
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 圖片二進位資料，可為 null
    /// </summary>
    public byte[]? Picture { get; set; }

    /// <summary>
    /// 反向導航：一個 Category 對應多個 Product
    /// </summary>
    public ICollection<Product> Products { get; private set; } = new List<Product>();
}
