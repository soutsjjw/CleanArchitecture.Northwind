using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities.Identity;

public class ApplicationUserProfile : IAuditableEntity
{
    [Key]
    public string UserId { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// 身分證號
    /// </summary>
    public string? IDNo { get; set; }

    /// <summary>
    // 職稱
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 一級機關
    /// </summary>
    public int Department { get; set; }

    /// <summary>
    /// 二級機關
    /// </summary>
    public int Office { get; set; }

    /// <summary>
    /// 帳號狀態
    /// </summary>
    public Status Status { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }
}
