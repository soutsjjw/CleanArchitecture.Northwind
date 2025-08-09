using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities.Identity;

public class ApplicationUserProfile : BaseAuditableEntity<int>, IAuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override int Id { get; set; }

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
    /// 性別
    /// </summary>
    public Gender Gender { get; set; } = Gender.Unknow;

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
    /// 取得或設定一個值，該值指示是否啟用基於時間的一次性密碼 (TOTP) 驗證。
    /// </summary>
    public bool IsTotpEnabled { get; set; }

    /// <summary>
    /// 取得或設定用於產生基於時間的一次性密碼的 TOTP 金鑰。
    /// </summary>
    public string? TotpSecretKey { get; set; }

    /// <summary>
    /// 取得或設定 TOTP 恢復代碼。
    /// </summary>
    public string? TotpRecoveryCodes { get; set; }

    /// <summary>
    /// 帳號狀態
    /// </summary>
    public Status Status { get; set; }

    /// <summary>
    /// 取得或設定實體的建立日期和時間。
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// 取得或設定建立實體的使用者的識別碼。
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 取得或設定項目最後修改的日期和時間。
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// 取得或設定最後修改實體的使用者的識別碼。
    /// </summary>
    public string? LastModifiedBy { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }
}
